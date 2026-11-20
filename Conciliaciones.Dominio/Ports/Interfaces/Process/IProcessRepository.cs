using Conciliaciones.Dominio.DTOs.Response.Process;

namespace Conciliaciones.Dominio.Ports.Interfaces.Process
{
    public interface IProcessRepository
    {
        Task<List<ProcesstSummaryDto>> GetProcessesByIdProjectsAsync(string proyectoId, CancellationToken cancellationToken);

    }
}
