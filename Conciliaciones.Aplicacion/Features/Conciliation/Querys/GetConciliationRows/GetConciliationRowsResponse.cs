namespace Conciliaciones.Aplicacion.Features.Conciliation.Querys.GetConciliationRows;

public record GetConciliationRowsResponse(
   long casosTotales,
   decimal montoAfectado,
   long totalCount,
   long totalPages,
   int pageNumber,
   int pageSize,
   List<Dictionary<string, object?>> data
);


