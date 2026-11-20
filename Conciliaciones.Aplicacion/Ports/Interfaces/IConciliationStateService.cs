using Conciliaciones.Dominio.DTOs.Response.Conciliation;

namespace Conciliaciones.Aplicacion.Ports.Interfaces
{
    public interface IConciliationStateService
    {
        Task<StatusUpdateConciliation> UpdateEstadoAsync(List<Guid> conciliacionId, CancellationToken cancellationToken);
    }
}
