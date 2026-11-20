using Conciliaciones.Dominio.DTOs.Response.Conciliation;
using Conciliaciones.Infraestructura.Persistence.Documents.Projections;

namespace Conciliaciones.Infraestructura.Ports.Interfaces.Persistence
{
    public interface IConciliationRowsComposer
    {
        PagedRowsDto Compose(ConciliationRowsAggregationProjection? groupResult, int pageNumber, int pageSize);
    }
}
