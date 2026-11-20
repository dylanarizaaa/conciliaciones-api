using Conciliaciones.Dominio.DTOs.Response.Process;
using Conciliaciones.Dominio.Envoltorios;
using Conciliaciones.Dominio.Ports.Interfaces.Process;
using MediatR;

namespace Conciliaciones.Aplicacion.Features.Process.Querys.GetProcessByIdProjects
{
    public class GetProcessByIdProjectsHandler : IRequestHandler<GetProcessByIdProjectsQuery, Answers<GetProcessByIdProjectsResponse>>
    {
        private readonly IProcessRepository _processRepository;

        public GetProcessByIdProjectsHandler(IProcessRepository processRepository)
        {
            _processRepository = processRepository ?? throw new ArgumentNullException(nameof(processRepository), "El repositorio de conciliaciones no puede ser nulo."); ;
        }
        public async Task<Answers<GetProcessByIdProjectsResponse>> Handle(GetProcessByIdProjectsQuery request, CancellationToken cancellationToken)
        {
            if (request == null)
            {
                throw new ArgumentNullException(nameof(request), "La consulta no puede ser nula.");
            }
            List<ProcesstSummaryDto> listaResumenes = await _processRepository.GetProcessesByIdProjectsAsync(request.ProyectoId, cancellationToken);

            decimal montoTotalGlobal = listaResumenes != null ? listaResumenes.Sum(p => p.Monto) : 0m;
            long cantidadTotalCasos = listaResumenes != null ? listaResumenes.Sum(p => p.Cantidad) : 0L;

            GetProcessByIdProjectsResponse response = new GetProcessByIdProjectsResponse(
                totalAfectado: montoTotalGlobal,
                casosTotales: cantidadTotalCasos,
                data: listaResumenes ?? new List<ProcesstSummaryDto>()
            );
            return new Answers<GetProcessByIdProjectsResponse>(response);
        }

    }
}
