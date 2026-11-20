namespace Conciliaciones.Infraestructura.Settings
{
    public static class DocumentDbSettingsFactory
    {
        public static DocumentDbSettings FromEnvironment(string connectionString)
        {
            if (string.IsNullOrWhiteSpace(connectionString))
            {
                throw new ArgumentException("La cadena de conexión de DocumentDB no puede estar vacía.", nameof(connectionString));
            }

            string databaseEnv = Environment.GetEnvironmentVariable("DATABASE")
                ?? throw new ArgumentNullException("DATABASE", "La variable de entorno DATABASE no está configurada.");

            // USESSL canonical; USSSSL legacy alias. Default false (mismo comportamiento previo al hardening).
            string sslEnv = Environment.GetEnvironmentVariable("USESSL")
                ?? Environment.GetEnvironmentVariable("USSSSL")
                ?? "false";
            bool.TryParse(sslEnv, out bool useSsl);

            string? rawCertPath = Environment.GetEnvironmentVariable("CACERTIFICATEPATH");
            string certPath = ResolveCaCertificatePath(rawCertPath);

            return new DocumentDbSettings
            {
                Provider = "MongoDB",
                ConnectionString = connectionString,
                DatabaseName = databaseEnv,
                UseSsl = useSsl,
                CaCertificatePath = certPath
            };
        }

        /// <summary>
        /// Resuelve el PEM en rutas típicas (output, Api/Certificados, C:\Certificados) para no romper TLS.
        /// </summary>
        /// <summary>Hook de prueba para simular File.Exists sin tocar el disco.</summary>
        public static Func<string, bool> FileExists { get; set; } = File.Exists;

        public static string ResolveCaCertificatePath(string? configuredPath)
        {
            List<string> candidates = new List<string>();
            if (!string.IsNullOrWhiteSpace(configuredPath))
            {
                candidates.Add(configuredPath);
                if (!Path.IsPathRooted(configuredPath))
                {
                    candidates.Add(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, configuredPath));
                    candidates.Add(Path.Combine(Directory.GetCurrentDirectory(), configuredPath));
                }
            }

            candidates.Add(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Certificados", "global-bundle.pem"));
            candidates.Add(Path.Combine(Directory.GetCurrentDirectory(), "Certificados", "global-bundle.pem"));
            candidates.Add(@"C:\Certificados\global-bundle.pem");

            foreach (string candidate in candidates.Distinct(StringComparer.OrdinalIgnoreCase))
            {
                if (FileExists(candidate))
                {
                    return candidate;
                }
            }

            return Path.IsPathRooted(configuredPath ?? string.Empty)
                ? configuredPath!
                : Path.Combine(AppDomain.CurrentDomain.BaseDirectory, configuredPath ?? Path.Combine("Certificados", "global-bundle.pem"));
        }
    }
}
