using Conciliaciones.Dominio.DTOs.Response.Projects;

namespace Conciliaciones.Dominio.Ports.Interfaces.Proyects
{
    public interface IProyectsReportRepository
    {
        Task<List<ProjectSummaryDto>> GetResumenProyectosAsync(CancellationToken cancellationToken);
    }
}
