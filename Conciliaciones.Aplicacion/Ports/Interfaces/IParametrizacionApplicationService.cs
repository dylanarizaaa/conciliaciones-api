using Conciliaciones.Dominio.DTOs.Requests.Conciliation;
using Conciliaciones.Dominio.DTOs.Response.Conciliation;

namespace Conciliaciones.Aplicacion.Ports.Interfaces
{
    public interface IParametrizacionApplicationService
    {
        Task<CreateParametrizacionResponse> CreateAsync(CreateParametrizacionRequest request, CancellationToken cancellationToken);
    }
}
