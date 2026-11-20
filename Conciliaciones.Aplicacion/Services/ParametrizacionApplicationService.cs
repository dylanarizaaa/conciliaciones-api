using Conciliaciones.Aplicacion.Ports.Interfaces;
using Conciliaciones.Dominio.DTOs.Requests.Conciliation;
using Conciliaciones.Dominio.DTOs.Response.Conciliation;
using Conciliaciones.Dominio.Ports.Interfaces.Conciliations;

namespace Conciliaciones.Aplicacion.Services
{
    public sealed class ParametrizacionApplicationService : IParametrizacionApplicationService
    {
        private readonly IParametrizacionWritePort _writePort;

        public ParametrizacionApplicationService(IParametrizacionWritePort writePort)
        {
            _writePort = writePort ?? throw new ArgumentNullException(nameof(writePort));
        }

        public Task<CreateParametrizacionResponse> CreateAsync(CreateParametrizacionRequest request, CancellationToken cancellationToken)
        {
            return _writePort.CreateParametrizacionAsync(request, cancellationToken);
        }
    }
}
