using Conciliaciones.Aplicacion.Ports.Interfaces;
using Conciliaciones.Dominio.DTOs.Response.Conciliation;
using Conciliaciones.Dominio.Envoltorios;
using MediatR;

namespace Conciliaciones.Aplicacion.Features.Conciliation.Querys.GetConciliationsByIdSp
{
    public class GetConciliationsByIdSpHandler : IRequestHandler<GetConciliationsByIdSpQuery, Answers<GetConciliationsByIdSpResponse>>
    {
        private readonly IConciliationQueryService _conciliationQueryService;

        public GetConciliationsByIdSpHandler(IConciliationQueryService conciliationQueryService)
        {
            _conciliationQueryService = conciliationQueryService ?? throw new ArgumentNullException(nameof(conciliationQueryService), "El repositorio de conciliaciones no puede ser nulo.");
        }

        public async Task<Answers<GetConciliationsByIdSpResponse>> Handle(GetConciliationsByIdSpQuery request, CancellationToken cancellationToken)
        {
            if (request == null)
            {
                throw new ArgumentNullException(nameof(request), "La consulta no puede ser nula.");
            }

            ColumsDetailsDto resultado = await _conciliationQueryService.GetConciliationByIdSp(
                request.ReglaId,
                cancellationToken
            );

            GetConciliationsByIdSpResponse response = new GetConciliationsByIdSpResponse(
                blocks: resultado?.Blocks?.ToList() ?? new List<BlockUiDto>()
            );

            return new Answers<GetConciliationsByIdSpResponse>(response);
        }
    }
}
