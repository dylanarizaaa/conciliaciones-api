using Conciliaciones.Infraestructura.Settings;
using MongoDB.Driver;
using System.Security.Cryptography.X509Certificates;

namespace Conciliaciones.Infraestructura.Persistence.DocumentDB
{
    public static class DocumentDbTlsConfigurator
    {
        public static void Apply(MongoClientSettings mongoSettings, DocumentDbSettings config)
        {
            if (config.UseSsl != true)
            {
                return;
            }

            mongoSettings.UseTls = true;
            string certPath = config.CaCertificatePath ?? "global-bundle.pem";
            if (string.IsNullOrEmpty(certPath) || !File.Exists(certPath))
            {
                return;
            }

            X509Certificate2Collection caCerts = new X509Certificate2Collection();
            caCerts.Import(certPath);
            mongoSettings.SslSettings = new SslSettings
            {
                ServerCertificateValidationCallback = (_, certificate, _, sslPolicyErrors) =>
                    ValidateCertificate(certificate, caCerts, sslPolicyErrors)
            };
        }

        private static bool ValidateCertificate(
            System.Security.Cryptography.X509Certificates.X509Certificate? certificate,
            X509Certificate2Collection caCerts,
            System.Net.Security.SslPolicyErrors sslPolicyErrors)
        {
            if (sslPolicyErrors == System.Net.Security.SslPolicyErrors.None)
            {
                return true;
            }

            using X509Chain customChain = new X509Chain();
            customChain.ChainPolicy.ExtraStore.AddRange(caCerts);
            customChain.ChainPolicy.RevocationMode = X509RevocationMode.NoCheck;
            customChain.ChainPolicy.VerificationFlags = X509VerificationFlags.AllowUnknownCertificateAuthority;
            return customChain.Build((X509Certificate2)certificate!);
        }
    }
}
