using Conciliaciones.Infraestructura.Ports.Interfaces.DBContext;
using Conciliaciones.Infraestructura.Ports.Interfaces.Persistence;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Conciliaciones.Infraestructura.Persistence.DocumentDB
{
    public sealed class DocumentDbStartupHostedService : IHostedService
    {
        private readonly IDocumentDBContext _context;
        private readonly IDocumentDbIndexInitializer _indexInitializer;
        private readonly ILogger<DocumentDbStartupHostedService> _logger;

        public DocumentDbStartupHostedService(
            IDocumentDBContext context,
            IDocumentDbIndexInitializer indexInitializer,
            ILogger<DocumentDbStartupHostedService> logger)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _indexInitializer = indexInitializer ?? throw new ArgumentNullException(nameof(indexInitializer));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            await _context.InitializeAsync(cancellationToken);

            try
            {
                await _indexInitializer.EnsureIndexesAsync(cancellationToken);
            }
            catch (Exception ex)
            {
                // Index creation may require elevated privileges; log loudly but keep serving if ping works.
                _logger.LogError(ex, "No se pudieron asegurar índices DocumentDB en el arranque.");
            }
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }
}
