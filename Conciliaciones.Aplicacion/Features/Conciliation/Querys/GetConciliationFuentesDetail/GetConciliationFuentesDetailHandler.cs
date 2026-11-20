using Conciliaciones.Aplicacion.Ports.Interfaces;
using Conciliaciones.Dominio.DTOs.Response.Conciliation;
using Conciliaciones.Dominio.Envoltorios;
using MediatR;

namespace Conciliaciones.Aplicacion.Features.Conciliation.Querys.GetConciliationFuentesDetail
{
    public class GetConciliationFuentesDetailHandler : IRequestHandler<GetConciliationFuentesDetailQuery, Answers<GetConciliationFuentesDetailResponse>>
    {
        private readonly IConciliationQueryService _conciliationQueryService;

        public GetConciliationFuentesDetailHandler(IConciliationQueryService conciliationQueryService)
        {
            _conciliationQueryService = conciliationQueryService ?? throw new ArgumentNullException(nameof(conciliationQueryService), "El repositorio de conciliaciones no puede ser nulo.");
        }

        public async Task<Answers<GetConciliationFuentesDetailResponse>> Handle(GetConciliationFuentesDetailQuery request, CancellationToken cancellationToken)
        {
            if (request == null)
            {
                throw new ArgumentNullException(nameof(request), "La consulta no puede ser nula.");
            }

            List<FuenteDetalleDto> data = await _conciliationQueryService.GetConciliationFuentesDetailAsync(request.idConciliacion, cancellationToken);
            GetConciliationFuentesDetailResponse response = new GetConciliationFuentesDetailResponse(data);

            return new Answers<GetConciliationFuentesDetailResponse>(response);
        }
    }
}
