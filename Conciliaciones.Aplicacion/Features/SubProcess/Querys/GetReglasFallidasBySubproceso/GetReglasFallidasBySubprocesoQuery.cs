using Conciliaciones.Dominio.Envoltorios;
using MediatR;

namespace Conciliaciones.Aplicacion.Features.SubProcess.Querys.GetReglasFallidasBySubproceso;

public record GetReglasFallidasBySubprocesoQuery(
    string subProcesoId,
    int pageNumber = 1,
    int pageSize = 10
) : IRequest<Answers<GetReglasFallidasBySubprocesoResponse>>;

