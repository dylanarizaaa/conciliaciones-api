using Conciliaciones.Dominio.DTOs.Response.Projects;
using Conciliaciones.Dominio.Envoltorios;
using Conciliaciones.Dominio.Ports.Interfaces.Proyects;
using MediatR;

namespace Conciliaciones.Aplicacion.Features.Projects.Querys.GetResumenProyectos
{
    public class GetResumenProyectosHandler : IRequestHandler<GetResumenProyectosQuery, Answers<GetResumenProyectosResponse>>
    {
        private readonly IProyectsReportRepository _proyectoReportRepository;

        public GetResumenProyectosHandler(IProyectsReportRepository proyectoReportRepository)
        {
            _proyectoReportRepository = proyectoReportRepository ?? throw new ArgumentNullException(nameof(proyectoReportRepository), "El repositorio de conciliaciones no puede ser nulo."); ;
        }

        public async Task<Answers<GetResumenProyectosResponse>> Handle(GetResumenProyectosQuery request, CancellationToken cancellationToken)
        {
            if (request == null)
            {
                throw new ArgumentNullException(nameof(request), "La consulta no puede ser nula.");
            }
            List<ProjectSummaryDto> listaResumenes = await _proyectoReportRepository.GetResumenProyectosAsync(cancellationToken);
            GetResumenProyectosResponse response = new GetResumenProyectosResponse(listaResumenes);
            return new Answers<GetResumenProyectosResponse>(response);
        }

    }
}
