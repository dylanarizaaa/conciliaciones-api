namespace Conciliaciones.Dominio.DTOs.Response.Conciliation;

public record StatusUpdateConciliation(
     List<Guid> Actualizados,
     List<Guid> NoEncontrados,
     int TotalProcesados);

