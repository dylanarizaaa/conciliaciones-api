using Conciliaciones.Dominio.Envoltorios;
using MediatR;

namespace Conciliaciones.Aplicacion.Features.Conciliation.Commands.UpdateConciliacionEstado;

public record UpdateConciliacionEstadoCommand(List<Guid> ConciliacionId) : IRequest<Answers<UpdateConciliacionEstadoResponse>>;

