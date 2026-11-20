namespace Conciliaciones.Dominio.DTOs.Response.Automatizaciones;

public record CreateAutomatizacionResponse(
    string AutomatizacionId,
    string ParametrizacionId,
    string Mensaje
);
