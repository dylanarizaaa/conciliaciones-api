namespace Conciliaciones.Dominio.DTOs.Response.ExternalAPIs;

public record RulesParamsResponseDto
(
    string idParametrizacion,
    List<Dictionary<string, string>>? parametros,
    List<RuleParamDto> reglas
);

public record RuleParamDto(
    string IdRegla,
    string NombreRegla
);

