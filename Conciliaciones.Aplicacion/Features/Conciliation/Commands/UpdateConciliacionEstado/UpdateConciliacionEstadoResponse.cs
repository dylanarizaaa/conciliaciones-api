namespace Conciliaciones.Aplicacion.Features.Conciliation.Commands.UpdateConciliacionEstado;

public record UpdateConciliacionEstadoResponse(
   List<Guid> actualizados,
   List<Guid> noEncontrados,
   int totalProcesados);
