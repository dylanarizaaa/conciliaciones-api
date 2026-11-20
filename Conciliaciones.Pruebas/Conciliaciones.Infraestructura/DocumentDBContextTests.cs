using Conciliaciones.Infraestructura.Persistence.DocumentDB;
using Conciliaciones.Infraestructura.Settings;
using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Driver;

namespace Conciliaciones.Pruebas.Conciliaciones.Infraestructura.Persistence.DocumentDB
{
    [TestClass]
    public class DocumentDBContextTests
    {
        [TestMethod]
        public void Constructor_ConSettingsNulo_LanzaArgumentNullException()
        {
            // Arrange & Act & Assert
            Assert.ThrowsExactly<ArgumentNullException>(() => new DocumentDBContext((IOptions<DocumentDbSettings>)null!));
        }

        [TestMethod]
        public void Constructor_ConConnectionStringVacio_LanzaArgumentException()
        {
            // Arrange
            DocumentDbSettings settingsInvalidos = new DocumentDbSettings
            {
                ConnectionString = string.Empty,
                DatabaseName = "TestDb"
            };
            IOptions<DocumentDbSettings> options = Options.Create(settingsInvalidos);

            // Act & Assert
            Assert.ThrowsExactly<ArgumentException>(() => new DocumentDBContext(options));
        }

        [TestMethod]
        public void Constructor_ConSettingsValidos_InicializaDatabaseCorrectamente()
        {
            // Arrange
            DocumentDbSettings settingsValidos = new DocumentDbSettings
            {
                ConnectionString = "mongodb://localhost:27017",
                DatabaseName = "ConciliacionesTestDb",
                UseSsl = false
            };
            IOptions<DocumentDbSettings> options = Options.Create(settingsValidos);

            // Act
            DocumentDBContext context = new DocumentDBContext(options);

            // Assert
            Assert.IsNotNull(context);
            Assert.IsNotNull(context.Database);
            Assert.AreEqual("ConciliacionesTestDb", context.Database.DatabaseNamespace.DatabaseName);
        }

        [TestMethod]
        public void GetCollection_NombreColeccionValido_RetornaInstanciaIMongoCollection()
        {
            // Arrange
            DocumentDbSettings settingsValidos = new DocumentDbSettings
            {
                ConnectionString = "mongodb://localhost:27017",
                DatabaseName = "ConciliacionesTestDb",
                UseSsl = false
            };
            IOptions<DocumentDbSettings> options = Options.Create(settingsValidos);
            DocumentDBContext context = new DocumentDBContext(options);
            string nombreColeccion = "conciliaciones";

            // Act
            IMongoCollection<BsonDocument> collection = context.GetCollection<BsonDocument>(nombreColeccion);

            // Assert
            Assert.IsNotNull(collection);
            Assert.AreEqual(nombreColeccion, collection.CollectionNamespace.CollectionName);
        }

        [TestMethod]
        public void GetCollection_NombreColeccionNuloOVacio_LanzaArgumentNullException()
        {
            // Arrange
            DocumentDbSettings settingsValidos = new DocumentDbSettings
            {
                ConnectionString = "mongodb://localhost:27017",
                DatabaseName = "ConciliacionesTestDb",
                UseSsl = false
            };
            IOptions<DocumentDbSettings> options = Options.Create(settingsValidos);
            DocumentDBContext context = new DocumentDBContext(options);

            // Act & Assert
            Assert.ThrowsExactly<ArgumentNullException>(() => context.GetCollection<BsonDocument>(null!));
            Assert.ThrowsExactly<ArgumentNullException>(() => context.GetCollection<BsonDocument>(string.Empty));
        }
    }
}