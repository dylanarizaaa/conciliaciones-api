using Conciliaciones.Infraestructura.Ports.Interfaces.DBContext;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using MongoDB.Bson;
using MongoDB.Driver;

namespace Conciliaciones.Infraestructura.Adapters
{
    public sealed class DocumentDbReadyHealthCheck : IHealthCheck
    {
        private readonly IDocumentDBContext _dbContext;

        public DocumentDbReadyHealthCheck(IDocumentDBContext dbContext)
        {
            _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        }

        public async Task<HealthCheckResult> CheckHealthAsync(
            HealthCheckContext context,
            CancellationToken cancellationToken = default)
        {
            try
            {
                BsonDocument command = new BsonDocument("ping", 1);
                await _dbContext.Database.RunCommandAsync<BsonDocument>(command, cancellationToken: cancellationToken);
                return HealthCheckResult.Healthy("DocumentDB ping OK");
            }
            catch (Exception ex)
            {
                return HealthCheckResult.Unhealthy("DocumentDB no disponible", ex);
            }
        }
    }
}