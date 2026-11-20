using Conciliaciones.Dominio.Envoltorios;
using MediatR;

namespace Conciliaciones.Aplicacion.Features.Conciliation.Querys.GetConciliationFuentesDetail;

public record GetConciliationFuentesDetailQuery(
    string idConciliacion
 ) : IRequest<Answers<GetConciliationFuentesDetailResponse>>;

