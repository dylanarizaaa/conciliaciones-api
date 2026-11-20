using Conciliaciones.Dominio.DTOs.Requests.Automatizaciones;
using Conciliaciones.Dominio.Envoltorios;
using MediatR;

namespace Conciliaciones.Aplicacion.Features.Automatizaciones.Commands.CreateAutomatizacion;

public record CreateAutomatizacionCommand(
    CreateAutomatizacionRequest Request
) : IRequest<Answers<CreateAutomatizacionResponse>>;
