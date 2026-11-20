using Conciliaciones.Dominio.DTOs.Response.Conciliation;

namespace Conciliaciones.Dominio.Ports.Interfaces.Conciliations
{
    public interface IConciliationStatePort
    {
        Task<StatusUpdateConciliation> UpdateEstadoConciliacionAsync(List<Guid> conciliacionId, CancellationToken cancellationToken);
    }
}
