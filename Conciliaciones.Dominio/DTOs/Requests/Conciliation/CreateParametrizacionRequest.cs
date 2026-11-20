namespace Conciliaciones.Dominio.DTOs.Requests.Conciliation
{
    public class CreateParametrizacionRequest
    {
        public string? proyecto { get; set; }
        public string? proceso { get; set; }
        public string? subproceso { get; set; }

        public ParametrizacionBodyDto? parametrizacion { get; set; }
        public parametrizacionUiBodyDto? parametrizacionUI { get; set; }
    }

    public class ParametrizacionBodyDto
    {
        public List<ParametroDto>? parametros { get; set; }
        public List<FuenteParametrizacionDto>? fuentes { get; set; }
        public List<TransformacionDto>? transformacion { get; set; }
        public List<ReglaItemDto>? reglas { get; set; }
    }

    public class ParametroDto
    {
        public string? nombre { get; set; }
        public string? tipo { get; set; }
    }

    public class FuenteParametrizacionDto
    {
        public string? fuenteId { get; set; }
        public string? conexionId { get; set; }
        public string? tabla { get; set; }
        public List<campoSeleccionDto>? camposSeleccion { get; set; }

        public List<datosFiltros>? filtros { get; set; }
    }

    public class datosFiltros
    {
        public string? operador { get; set; }

        public List<listCondiciones>? condiciones { get; set; }
    }

    public class listCondiciones
    {
        public string? campo { get; set; }
        public string? operador { get; set; }
        public object? valor { get; set; }
    }

    public class campoSeleccionDto
    {
        public string? nombre { get; set; }
        public string? tipoDato { get; set; }
        public string? alias { get; set; }
    }

    public class TransformacionDto
    {
        public string? alias { get; set; }
        public string? tipo { get; set; }
        public string? operacion { get; set; }
        public List<OperandoDto>? operandos { get; set; }
    }

    public class OperandoDto
    {
        public string? @ref { get; set; }
    }



    public class ReglaItemDto
    {
        public string idRegla { get; set; } = string.Empty;
        public string nombreRegla { get; set; } = string.Empty;
        public string eventoExito { get; set; } = string.Empty;
        public string mensajeError { get; set; } = string.Empty;
        public string expresion { get; set; } = string.Empty;
    }

    public class parametrizacionUiBodyDto
    {
        public List<fuenteUiColumnaInputDto> fuente { get; set; } = new();
        public List<fuenteUiDatosInputDto> datos { get; set; } = new();
    }

    public class fuenteUiColumnaInputDto
    {
        public string fuenteDato { get; set; } = string.Empty;
        public string tipo { get; set; } = string.Empty;
        public string label { get; set; } = string.Empty;
    }

    public class fuenteUiDatosInputDto
    {
        public string idFuente { get; set; } = string.Empty;
        public List<fuenteUiColumnaInputDto> datos { get; set; } = new();
    }
}