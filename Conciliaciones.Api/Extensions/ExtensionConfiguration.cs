using IntegracionSeguridadKeycloak.Core.Shared.Configuraciones;
using IntegracionSeguridadKeycloak.Core.Shared.Seguridad;
using System.Diagnostics.CodeAnalysis;

namespace Conciliaciones.Api.Extensions
{
    [ExcludeFromCodeCoverage]
    public static class ExtensionConfiguration
    {
        public static AppConfiguracion AddAppConfiguration(this IServiceCollection services)
        {
            AppConfiguracion appConfiguracion = BuildAppConfiguracion();
            services.AddSingleton(appConfiguracion);
            return appConfiguracion;
        }

        private static AppConfiguracion BuildAppConfiguracion()
        {
            (string Authority, string Audience, bool RequireHttps, string RequiredScope, string RequiredRoles) keycloak =
                ReadKeycloakSettings();
            (string LlaveCifrada, string Pdb) secrets = ReadDecodedSecrets();

            return new AppConfiguracion
            {
                KeycloakAuthority = keycloak.Authority,
                KeycloakAudience = keycloak.Audience,
                KeycloakRequireHttps = keycloak.RequireHttps,
                RequiredScope = keycloak.RequiredScope,
                RequiredRoles = keycloak.RequiredRoles,
                LLAVE_CIFRADA = secrets.LlaveCifrada,
                PDB = secrets.Pdb
            };
        }

        private static (string Authority, string Audience, bool RequireHttps, string RequiredScope, string RequiredRoles) ReadKeycloakSettings()
        {
            string authority = RequireEnv("KEYCLOAK_AUTHORITY");
            string audience = RequireEnv("KEYCLOAK_AUDIENCE");
            // Secure default: require HTTPS metadata unless explicitly disabled with "false".
            string requireHttpsRaw = Environment.GetEnvironmentVariable("KEYCLOAK_REQUIRE_HTTPS_METADATA") ?? string.Empty;
            bool requireHttps = !string.Equals(requireHttpsRaw, "false", StringComparison.OrdinalIgnoreCase);
            string requiredScope = Environment.GetEnvironmentVariable("REQUIRED_SCOPE") ?? "interpay";
            string requiredRoles = RequireEnv("REQUIRED_ROLE");
            return (authority, audience, requireHttps, requiredScope, requiredRoles);
        }

        private static (string LlaveCifrada, string Pdb) ReadDecodedSecrets()
        {
            string llaveCifrada = Encripcion.Base64Decode(RequireEnv("LLAVE_CIFRADA"));
            string pdb = Encripcion.Base64Decode(RequireEnv("PDB"));
            return (llaveCifrada, pdb);
        }

        private static string RequireEnv(string name) =>
            Environment.GetEnvironmentVariable(name)
            ?? throw new ArgumentNullException(name, $"La variable {name} es requerida.");
    }
}
