using Conciliaciones.Infraestructura.Ports.Interfaces.AWS;
using Conciliaciones.Infraestructura.Ports.Interfaces.DBContext;
using Conciliaciones.Infraestructura.Settings;
using Microsoft.Extensions.Options;
using MongoDB.Driver;

namespace Conciliaciones.Infraestructura.Persistence.DocumentDB
{
    public class DocumentDBContext : IDocumentDBContext
    {
        private readonly IParameterStoreService? _parameterStore;
        private IMongoDatabase? _database;

        public virtual IMongoDatabase Database => _database
            ?? throw new InvalidOperationException("DocumentDBContext no ha sido inicializado.");

        public DocumentDBContext()
        {
        }

        public DocumentDBContext(IOptions<DocumentDbSettings> settings)
        {
            if (settings == null || settings.Value == null)
            {
                throw new ArgumentNullException(nameof(settings), "La configuración de DocumentDbSettings no puede ser nula.");
            }

            Configure(settings.Value);
        }

        public DocumentDBContext(IParameterStoreService parameterStore)
        {
            _parameterStore = parameterStore ?? throw new ArgumentNullException(nameof(parameterStore));
        }

        public async Task InitializeAsync(CancellationToken cancellationToken)
        {
            if (_database != null || _parameterStore == null)
            {
                return;
            }

            string connectionString = await _parameterStore.GetValueParameterStore("ConexionMotorConciliaciones", cancellationToken);
            DocumentDbSettings settings = DocumentDbSettingsFactory.FromEnvironment(connectionString);
            Configure(settings);
        }

        public virtual IMongoCollection<T> GetCollection<T>(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentNullException(nameof(name), "El nombre de la colección no puede ser nulo ni estar vacío.");
            }

            return Database.GetCollection<T>(name);
        }

        private void Configure(DocumentDbSettings config)
        {
            if (string.IsNullOrWhiteSpace(config.ConnectionString))
            {
                throw new ArgumentException(
                    "La cadena de conexión (ConnectionString) de DocumentDB no puede estar vacía.",
                    nameof(config));
            }

            MongoClientSettings mongoSettings = MongoClientSettings.FromConnectionString(config.ConnectionString);
            DocumentDbTlsConfigurator.Apply(mongoSettings, config);
            MongoClient client = new MongoClient(mongoSettings);
            _database = client.GetDatabase(config.DatabaseName ?? "Conciliaciones");
        }
    }
}
