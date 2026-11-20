using Conciliaciones.Dominio.Entities.Common;

namespace Conciliaciones.Dominio.Entities.Parameterization
{
    public sealed class ParameterizationEntity
    {
        private ParameterizationEntity(
            string parametrizacionId,
            string subProcesoId,
            DateTime fechaCreacion,
            List<ParametroEntity>? parametros,
            List<FuenteEntity> fuentes,
            List<TransformacionEntity> transformacion,
            List<ReglaEntity>? reglas)
        {
            ParametrizacionId = parametrizacionId;
            SubProcesoId = subProcesoId;
            FechaCreacion = fechaCreacion;
            Parametros = parametros;
            Fuentes = fuentes;
            Transformacion = transformacion;
            Reglas = reglas;
        }

        public string ParametrizacionId { get; }

        public string SubProcesoId { get; }

        public DateTime FechaCreacion { get; }

        public IReadOnlyList<ParametroEntity>? Parametros { get; }

        public IReadOnlyList<FuenteEntity> Fuentes { get; }

        public IReadOnlyList<TransformacionEntity> Transformacion { get; }

        public IReadOnlyList<ReglaEntity>? Reglas { get; }

        public static ParameterizationEntity Create(
            string subProcesoId,
            string? parametrizacionId = null,
            IEnumerable<ParametroEntity>? parametros = null,
            IEnumerable<FuenteEntity>? fuentes = null,
            IEnumerable<TransformacionEntity>? transformacion = null,
            IEnumerable<ReglaEntity>? reglas = null)
        {
            return new ParameterizationEntity(
                string.IsNullOrWhiteSpace(parametrizacionId) ? EntityValidation.NewId() : parametrizacionId.Trim(),
                EntityValidation.Required(subProcesoId, nameof(subProcesoId)),
                DateTime.UtcNow,
                parametros?.ToList(),
                fuentes?.ToList() ?? new List<FuenteEntity>(),
                transformacion?.ToList() ?? new List<TransformacionEntity>(),
                reglas?.ToList() ?? new List<ReglaEntity>()
            );
        }
    }

    public sealed class ParametroEntity
    {
        public ParametroEntity(string? nombre, string? tipo)
        {
            Nombre = EntityValidation.OptionalTrim(nombre);
            Tipo = EntityValidation.OptionalTrim(tipo);
        }

        public string Nombre { get; }

        public string Tipo { get; }
    }

    public sealed class FuenteEntity
    {
        public FuenteEntity(
            string? fuenteId,
            string? conexionId,
            string? tabla,
            IEnumerable<CampoSeleccionEntity>? camposSeleccion = null,
            IEnumerable<FiltroEntity>? filtros = null)
        {
            FuenteId = EntityValidation.OptionalTrim(fuenteId);
            ConexionId = EntityValidation.OptionalTrim(conexionId);
            Tabla = EntityValidation.OptionalTrim(tabla);
            CamposSeleccion = camposSeleccion?.ToList();
            Filtros = filtros?.ToList();
        }

        public string FuenteId { get; }

        public string ConexionId { get; }

        public string Tabla { get; }

        public IReadOnlyList<CampoSeleccionEntity>? CamposSeleccion { get; }

        public IReadOnlyList<FiltroEntity>? Filtros { get; }
    }

    public sealed class CampoSeleccionEntity
    {
        public CampoSeleccionEntity(string? nombre, string? tipoDato, string? alias)
        {
            Nombre = EntityValidation.OptionalTrim(nombre);
            TipoDato = EntityValidation.OptionalTrim(tipoDato);
            Alias = EntityValidation.OptionalTrim(alias);
        }

        public string Nombre { get; }

        public string TipoDato { get; }

        public string Alias { get; }
    }

    public sealed class FiltroEntity
    {
        public FiltroEntity(string? operador, IEnumerable<CondicionFiltroEntity>? condiciones)
        {
            Operador = EntityValidation.OptionalTrim(operador);
            Condiciones = condiciones?.ToList();
        }

        public string Operador { get; }

        public IReadOnlyList<CondicionFiltroEntity>? Condiciones { get; }
    }

    public sealed class CondicionFiltroEntity
    {
        public CondicionFiltroEntity(string? campo, string? operador, object? valor)
        {
            Campo = EntityValidation.OptionalTrim(campo);
            Operador = EntityValidation.OptionalTrim(operador);
            Valor = valor;
        }

        public string Campo { get; }
        public string Operador { get; }
        public object? Valor { get; }
    }

    public sealed class TransformacionEntity
    {
        public TransformacionEntity(string? alias, string? tipo, string? operacion, IEnumerable<OperandoEntity>? operandos)
        {
            Alias = EntityValidation.OptionalTrim(alias);
            Tipo = EntityValidation.OptionalTrim(tipo);
            Operacion = EntityValidation.OptionalTrim(operacion);
            Operandos = operandos?.ToList() ?? new List<OperandoEntity>();
        }

        public string Alias { get; }

        public string Tipo { get; }

        public string Operacion { get; }

        public IReadOnlyList<OperandoEntity> Operandos { get; }
    }

    public sealed class OperandoEntity
    {
        public OperandoEntity(string? @ref)
        {
            Ref = EntityValidation.OptionalTrim(@ref);
        }

        public string Ref { get; }
    }

    public sealed class ReglaEntity
    {
        public ReglaEntity(
            string? nombreRegla,
            string? eventoExito,
            string? mensajeError,
            string? expresion,
            string? idRegla = null)
        {
            IdRegla = string.IsNullOrWhiteSpace(idRegla) ? EntityValidation.NewId() : idRegla.Trim();
            NombreRegla = EntityValidation.OptionalTrim(nombreRegla);
            EventoExito = EntityValidation.OptionalTrim(eventoExito);
            MensajeError = EntityValidation.OptionalTrim(mensajeError);
            Expresion = EntityValidation.OptionalTrim(expresion);
        }

        public string IdRegla { get; }

        public string NombreRegla { get; }

        public string EventoExito { get; }

        public string MensajeError { get; }

        public string Expresion { get; }
    }
}
