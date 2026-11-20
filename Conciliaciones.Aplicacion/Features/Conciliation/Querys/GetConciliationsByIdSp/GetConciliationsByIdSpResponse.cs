using Conciliaciones.Dominio.DTOs.Response.Conciliation;

namespace Conciliaciones.Aplicacion.Features.Conciliation.Querys.GetConciliationsByIdSp;

public record GetConciliationsByIdSpResponse(
   List<BlockUiDto> blocks
);

