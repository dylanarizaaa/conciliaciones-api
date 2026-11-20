namespace Conciliaciones.Dominio.DTOs.Response.Conciliation;

public record ConciliationResponseDto
(
  long CasosTotales,
decimal MontoAfectado,
int PageNumber,
int PageSize,
long TotalPages,
List<BlockUiDto> Blocks
);
public record CellConfigDto(
    string Type,
    Dictionary<string, string>? VariantByValue,
    Dictionary<string, object>? Config
);

public record ColumnUiDto(
    string Field,
    string Header,
    bool Sortable,
    CellConfigDto? Cell
);

public record TableConfigDto(
    List<ColumnUiDto> Columns,
    List<Dictionary<string, object>> Rows
);

public record BlockUiDto(
    string Id,
    string Type,
    string Title,
    TableConfigsDto Config
);

