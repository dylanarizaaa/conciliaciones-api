using Conciliaciones.Infraestructura.Persistence.Documents.Projections;

namespace Conciliaciones.Infraestructura.Ports.Interfaces.Persistence
{
    public interface IFailedConciliationSummaryMapper
    {
        (long Count, decimal MontoTotal) Summarize(IEnumerable<ConciliacionMontoProjection>? conciliaciones);
        decimal SumMontos(IEnumerable<ConciliacionMontoProjection>? conciliaciones);
    }
}
