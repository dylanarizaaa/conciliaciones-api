using Conciliaciones.Dominio.DTOs.Response.SubProcess;

namespace Conciliaciones.Dominio.Ports.Interfaces.SubProcess
{
    public interface ISubProcessRepository
    {
        Task<SubprocesoReglasResumenDto> GetReglasFallidasBySubprocesoIdAsync(string subProcesoId, int pageNumber, int pageSize, CancellationToken cancellationToken);

        Task<List<SubProcessInfoDto>> GetSubProcesosByIdProcess(string idProcess, CancellationToken cancellationToken);
    }
}
