namespace Conciliaciones.Infraestructura.Settings
{
    public class DocumentDbSettings
    {
        public string Provider { get; set; } = "MongoDB";
        public string? ConnectionString { get; set; }

        public string? DatabaseName { get; set; }

        public bool? UseSsl { get; set; }

        public string? CaCertificatePath { get; set; }

    }
}
