using Conciliaciones.Aplicacion.Ports.Interfaces;
using Conciliaciones.Dominio.DTOs.Response.Conciliation;
using Conciliaciones.Dominio.Ports.Interfaces.Conciliations;

namespace Conciliaciones.Aplicacion.Services
{
    public sealed class ConciliationStateService : IConciliationStateService
    {
        private readonly IConciliationStatePort _statePort;

        public ConciliationStateService(IConciliationStatePort statePort)
        {
            _statePort = statePort ?? throw new ArgumentNullException(nameof(statePort));
        }

        public Task<StatusUpdateConciliation> UpdateEstadoAsync(List<Guid> conciliacionId, CancellationToken cancellationToken)
        {
            return _statePort.UpdateEstadoConciliacionAsync(conciliacionId, cancellationToken);
        }
    }
}
