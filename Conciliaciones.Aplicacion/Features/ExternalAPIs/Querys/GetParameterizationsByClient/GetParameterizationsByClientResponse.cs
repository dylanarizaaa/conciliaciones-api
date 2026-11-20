using Conciliaciones.Dominio.DTOs.Response.ExternalAPIs;

namespace Conciliaciones.Aplicacion.Features.ExternalAPIs.Querys.GetParameterizationsByClient;

public record GetParameterizationsByClientResponse(
    List<RulesParamsResponseDto> Parametrizaciones
);
