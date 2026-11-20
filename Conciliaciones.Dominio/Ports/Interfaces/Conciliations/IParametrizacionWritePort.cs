using Conciliaciones.Dominio.DTOs.Requests.Conciliation;
using Conciliaciones.Dominio.DTOs.Response.Conciliation;

namespace Conciliaciones.Dominio.Ports.Interfaces.Conciliations
{
    public interface IParametrizacionWritePort
    {
        Task<CreateParametrizacionResponse> CreateParametrizacionAsync(CreateParametrizacionRequest request, CancellationToken cancellationToken);
    }
}
