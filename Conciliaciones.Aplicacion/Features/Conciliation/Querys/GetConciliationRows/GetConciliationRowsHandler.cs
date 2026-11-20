using Conciliaciones.Aplicacion.Ports.Interfaces;
using Conciliaciones.Dominio.DTOs.Response.Conciliation;
using Conciliaciones.Dominio.Entities.Common;
using Conciliaciones.Dominio.Envoltorios;
using MediatR;

namespace Conciliaciones.Aplicacion.Features.Conciliation.Querys.GetConciliationRows
{
    public class GetConciliationRowsHandler : IRequestHandler<GetConciliationRowsQuery, Answers<GetConciliationRowsResponse>>
    {
        private readonly IConciliationQueryService _conciliationQueryService;

        public GetConciliationRowsHandler(IConciliationQueryService conciliationQueryService)
        {
            _conciliationQueryService = conciliationQueryService ?? throw new ArgumentNullException(nameof(conciliationQueryService), "El repositorio de conciliaciones no puede ser nulo.");
        }

        public async Task<Answers<GetConciliationRowsResponse>> Handle(GetConciliationRowsQuery request, CancellationToken cancellationToken)
        {
            if (request == null)
            {
                throw new ArgumentNullException(nameof(request), "La consulta no puede ser nula.");
            }

            int pageNumber = PaginationLimits.NormalizePageNumber(request.PageNumber);
            int pageSize = PaginationLimits.NormalizePageSize(request.PageSize);
            PagedRowsDto pagedRowsDto = await _conciliationQueryService.GetConciliationRowsAsync(
                  request.ReglaId,
                  pageNumber,
                  pageSize,
                  cancellationToken
              );

            GetConciliationRowsResponse response = new GetConciliationRowsResponse(
                casosTotales: pagedRowsDto.CasosTotales,
                montoAfectado: pagedRowsDto.MontoAfectado,
                totalCount: pagedRowsDto.TotalCount,
                totalPages: pagedRowsDto.TotalPages,
                pageNumber: pagedRowsDto.PageNumber,
                pageSize: pagedRowsDto.PageSize,
                data: pagedRowsDto.Rows.ToList()
            );
            return new Answers<GetConciliationRowsResponse>(response);
        }
    }
}
