using Conciliaciones.Infraestructura.Settings;

namespace Conciliaciones.Pruebas.Conciliaciones.Infraestructura.Settings
{
    [TestClass]
    public class DocumentDbSettingsTests
    {
        [TestMethod]
        public void DocumentDbSettings_ValoresPorDefecto_InicializaCorrectamente()
        {
            // Arrange & Act
            DocumentDbSettings settings = new DocumentDbSettings();

            // Assert
            Assert.AreEqual("MongoDB", settings.Provider);
            Assert.IsNull(settings.ConnectionString);
            Assert.IsNull(settings.DatabaseName);
            Assert.IsNull(settings.UseSsl);
            Assert.IsNull(settings.CaCertificatePath);
        }

        [TestMethod]
        public void DocumentDbSettings_AsignacionPropiedades_AlmacenaValoresCorrectamente()
        {
            // Arrange
            string provider = "DocumentDB";
            string connectionString = "mongodb://localhost:27017";
            string databaseName = "ConciliacionesDb";
            bool useSsl = true;
            string caCertPath = "global-bundle.pem";

            // Act
            DocumentDbSettings settings = new DocumentDbSettings
            {
                Provider = provider,
                ConnectionString = connectionString,
                DatabaseName = databaseName,
                UseSsl = useSsl,
                CaCertificatePath = caCertPath
            };

            // Assert
            Assert.AreEqual(provider, settings.Provider);
            Assert.AreEqual(connectionString, settings.ConnectionString);
            Assert.AreEqual(databaseName, settings.DatabaseName);
            Assert.AreEqual(useSsl, settings.UseSsl);
            Assert.AreEqual(caCertPath, settings.CaCertificatePath);
        }
    }
}