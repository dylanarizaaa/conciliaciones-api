using Conciliaciones.Dominio.DTOs.Response.SubProcess;
using Conciliaciones.Dominio.Entities.Common;
using Conciliaciones.Dominio.Envoltorios;
using Conciliaciones.Dominio.Ports.Interfaces.SubProcess;
using MediatR;

namespace Conciliaciones.Aplicacion.Features.SubProcess.Querys.GetReglasFallidasBySubproceso
{
    public class GetReglasFallidasBySubprocesoHandler : IRequestHandler<GetReglasFallidasBySubprocesoQuery, Answers<GetReglasFallidasBySubprocesoResponse>>
    {

        private readonly ISubProcessRepository _subProcessRepository;

        public GetReglasFallidasBySubprocesoHandler(ISubProcessRepository subProcessRepository)
        {
            _subProcessRepository = subProcessRepository ?? throw new ArgumentNullException(nameof(subProcessRepository), "El repositorio de conciliaciones no puede ser nulo."); ;
        }

        public async Task<Answers<GetReglasFallidasBySubprocesoResponse>> Handle(GetReglasFallidasBySubprocesoQuery request, CancellationToken cancellationToken)
        {
            if (request == null)
            {
                throw new ArgumentNullException(nameof(request), "La consulta no puede ser nula.");
            }
            int pageNumber = PaginationLimits.NormalizePageNumber(request.pageNumber);
            int pageSize = PaginationLimits.NormalizePageSize(request.pageSize);

            SubprocesoReglasResumenDto result = await _subProcessRepository.GetReglasFallidasBySubprocesoIdAsync(
                request.subProcesoId,
                pageNumber,
                pageSize,
                cancellationToken
            );

            GetReglasFallidasBySubprocesoResponse response = new GetReglasFallidasBySubprocesoResponse(
                montoAfectado: result?.MontoAfectado ?? 0m,
                casosTotales: result?.CasosTotales ?? 0L,
                pageNumber: pageNumber,
                pageSize: pageSize,
                totalPages: result?.TotalPages ?? 0L,
                data: result?.Data?.ToList() ?? new List<FailedRuleDto>()
            );

            return new Answers<GetReglasFallidasBySubprocesoResponse>(response);
        }
    }
}
