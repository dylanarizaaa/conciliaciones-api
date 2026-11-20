using Conciliaciones.Dominio.Services;
using Conciliaciones.Infraestructura.Persistence.Documents.Projections;
using Conciliaciones.Infraestructura.Persistence.Mappers;
using Conciliaciones.Infraestructura.Ports.Interfaces.Persistence;

namespace Conciliaciones.Infraestructura.Persistence.Services
{
    public sealed class FailedConciliationSummaryMapper : IFailedConciliationSummaryMapper
    {
        private readonly ITransformacionMontoCalculator _montoCalculator;

        public FailedConciliationSummaryMapper(ITransformacionMontoCalculator montoCalculator)
        {
            _montoCalculator = montoCalculator ?? throw new ArgumentNullException(nameof(montoCalculator));
        }

        public (long Count, decimal MontoTotal) Summarize(IEnumerable<ConciliacionMontoProjection>? conciliaciones)
        {
            long count = conciliaciones?.Count() ?? 0;
            return (count, SumMontos(conciliaciones));
        }

        public decimal SumMontos(IEnumerable<ConciliacionMontoProjection>? conciliaciones)
        {
            return TransformacionMontoReader.SumFromConciliacionMontos(conciliaciones, _montoCalculator);
        }
    }
}
