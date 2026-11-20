namespace Conciliaciones.Dominio.DTOs.Response.Conciliation;

public record CreateParametrizacionResponse(
  string IdProyecto,
  string IdProceso,
  string IdSubProceso,
  string ParametrizacionId,
  string Mensaje
);

