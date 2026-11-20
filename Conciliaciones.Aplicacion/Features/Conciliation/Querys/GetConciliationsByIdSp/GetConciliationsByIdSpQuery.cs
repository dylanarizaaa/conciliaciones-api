using Conciliaciones.Dominio.Envoltorios;
using MediatR;

namespace Conciliaciones.Aplicacion.Features.Conciliation.Querys.GetConciliationsByIdSp;

public record GetConciliationsByIdSpQuery(
    string ReglaId
    ) : IRequest<Answers<GetConciliationsByIdSpResponse>>;
