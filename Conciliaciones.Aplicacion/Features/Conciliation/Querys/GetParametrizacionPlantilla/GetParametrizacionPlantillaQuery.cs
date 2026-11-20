using Conciliaciones.Dominio.Envoltorios;
using MediatR;

namespace Conciliaciones.Aplicacion.Features.Conciliation.Querys.GetParametrizacionPlantilla;

public record GetParametrizacionPlantillaQuery(
    string? ReglaId,
    string? TipoBlock
) : IRequest<Answers<GetParametrizacionPlantillaResponse>>;
