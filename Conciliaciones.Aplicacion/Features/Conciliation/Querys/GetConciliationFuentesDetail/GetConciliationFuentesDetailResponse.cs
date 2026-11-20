using Conciliaciones.Dominio.DTOs.Response.Conciliation;

namespace Conciliaciones.Aplicacion.Features.Conciliation.Querys.GetConciliationFuentesDetail;

public record GetConciliationFuentesDetailResponse(
    List<FuenteDetalleDto> data
);


