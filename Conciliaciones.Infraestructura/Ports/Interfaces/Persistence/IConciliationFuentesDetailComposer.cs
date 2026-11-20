using Conciliaciones.Dominio.DTOs.Response.Conciliation;
using Conciliaciones.Infraestructura.Persistence.Documents.Projections;

namespace Conciliaciones.Infraestructura.Ports.Interfaces.Persistence
{
    public interface IConciliationFuentesDetailComposer
    {
        List<FuenteDetalleDto> Compose(ConciliationFuentesDetailProjection? docResult);
    }
}
