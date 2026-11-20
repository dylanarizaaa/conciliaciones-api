using Conciliaciones.Dominio.Envoltorios;
using MediatR;

namespace Conciliaciones.Aplicacion.Features.Conciliation.Querys.GetConciliationRows;

public record GetConciliationRowsQuery(
     string ReglaId,
     int PageNumber = 1,
     int PageSize = 10
 ) : IRequest<Answers<GetConciliationRowsResponse>>;
