using Conciliaciones.Dominio.DTOs.Response.Reports;
using Conciliaciones.Infraestructura.Persistence.Documents.Projections;

namespace Conciliaciones.Infraestructura.Ports.Interfaces.Persistence
{
    public interface IConciliationExportComposer
    {
        ConciliacionExportDataDto Compose(
            ConciliationExportAggregationProjection? projection,
            string reglaId);
    }
}
