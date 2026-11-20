namespace Conciliaciones.Aplicacion.Features.Automatizaciones.Commands.CreateAutomatizacion;

public record CreateAutomatizacionResponse(
    string AutomatizacionId,
    string ParametrizacionId,
    string Mensaje
);
