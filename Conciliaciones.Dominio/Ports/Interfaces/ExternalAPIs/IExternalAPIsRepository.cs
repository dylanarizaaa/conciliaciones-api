using Conciliaciones.Dominio.DTOs.Response.ExternalAPIs;

namespace Conciliaciones.Dominio.Ports.Interfaces.ExternalAPIs
{
    public interface IExternalAPIsRepository
    {
        Task<IReadOnlyList<RulesParamsResponseDto>> GetParameterizationsByClientAsync(
            string? idProyecto,
            string? idProceso,
            string? idSubProceso,
            CancellationToken cancellationToken);
    }
}
