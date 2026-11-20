namespace Conciliaciones.Dominio.DTOs.Response.Conciliation;

public record ColumsDetailsDto
(

    List<BlockUiDto> Blocks
);


public record TableConfigsDto(
    List<ColumnUiDto> Columns
);
