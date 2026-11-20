namespace Conciliaciones.Infraestructura.Persistence.DocumentDb.Constants
{
    /// <summary>
    /// Nombres BSON reales del schema DocumentDB (sin magic strings en builders).
    /// </summary>
    public static class DocumentFields
    {
        public static class Common
        {
            public const string Nombre = "nombre";
            public const string FechaCreacion = "fechaCreacion";
            public const string Estado = "estado";
        }

        public static class Proyecto
        {
            public const string IdProyecto = "idProyecto";
            public const string Nombre = Common.Nombre;
            public const string FechaCreacion = Common.FechaCreacion;
        }

        public static class Proceso
        {
            public const string IdProceso = "idProceso";
            public const string IdProyecto = Proyecto.IdProyecto;
            public const string Nombre = Common.Nombre;
            public const string FechaCreacion = Common.FechaCreacion;
        }

        public static class SubProceso
        {
            public const string IdSubProceso = "idSubProceso";
            public const string IdProceso = Proceso.IdProceso;
            public const string Nombre = Common.Nombre;
            public const string FechaCreacion = Common.FechaCreacion;
        }

        public static class Parametrizacion
        {
            public const string ParametrizacionId = "parametrizacionId";
            public const string SubProcesoId = "subProcesoId";
            public const string FechaCreacion = Common.FechaCreacion;
            public const string Parametros = "parametros";
            public const string Fuentes = "fuentes";
            public const string Transformacion = "transformacion";
            public const string Reglas = "reglas";
        }

        public static class Reglas
        {
            public const string IdRegla = "idRegla";
            public const string NombreRegla = "nombreRegla";
            public const string EventoExito = "eventoExito";
            public const string MensajeError = "mensajeError";
            public const string Expresion = "expresion";
        }

        public static class ParametrizacionUi
        {
            public const string IdParametrizacion = "idParametrizacion";
            public const string Fuente = "fuente";
            public const string Datos = "datos";
            public const string Blocks = "blocks";
            public const string Field = "field";
            public const string Header = "header";
            public const string Sortable = "sortable";
            public const string Visible = "visible";
            public const string Order = "order";
            public const string CellConfig = "cellConfig";
            public const string Cell = "cell";
            public const string Type = "type";
            public const string Format = "format";
            public const string Config = "config";
            public const string VariantByValue = "variantByValue";
            public const string IdFuente = "idFuente";
            public const string Columns = "columns";
        }

        public static class Conciliacion
        {
            public const string ConciliacionId = "conciliacionId";
            public const string ParametrizacionId = Parametrizacion.ParametrizacionId;
            public const string Estado = Common.Estado;
            public const string FechaCreacion = Common.FechaCreacion;
            public const string ParametrosEntrada = "parametrosEntrada";
            public const string ResultadoTransformacion = "resultadoTransformacion";
            public const string ResultadoFuentes = "resultadoFuentes";
            public const string ResultadoRegla = "resultadoRegla";
            public const string NombreRegla = "nombreRegla";
            public const string FuenteId = "fuenteId";
            public const string ConexionId = "conexionId";
            public const string Data = "data";
        }

        public static class CurrencyConfig
        {
            public const string CurrencyCode = "currencyCode";
            public const string Display = "display";
            public const string DigitsInfo = "digitsInfo";
            public const string Locale = "locale";
            public const string Symbol = "symbol";
        }

        public static class AggregationAliases
        {
            public const string UiConfig = "uiConfig";
            public const string UiConfigDoc = "uiConfigDoc";
            public const string Conciliaciones = "conciliaciones";
            public const string ConciliacionesFiltradas = "conciliacionesFiltradas";
            public const string ConciliacionesPaginadas = "conciliacionesPaginadas";
            public const string ConciliacionesFallidas = "conciliacionesFallidas";
            public const string ParametrizacionDoc = "parametrizacionDoc";
            public const string SubprocesoDoc = "subprocesoDoc";
            public const string ProcesoDoc = "procesoDoc";
            public const string Procesos = "procesos";
            public const string SubProcesos = "subProcesos";
            public const string Parametrizaciones = "parametrizaciones";
            public const string TotalCount = "totalCount";
            public const string NombreProceso = "nombreProceso";
            public const string NombreProyecto = "nombreProyecto";
            public const string NombreSubProceso = "nombreSubProceso";
            public const string NombreRegla = "nombreRegla";
            public const string ReglaId = "idRegla";
            public const string ReglaSeleccionada = "reglaSeleccionada";
            public const string Casos = "casos";
            public const string CasosTotales = "casosTotales";
            public const string Nombre = "nombre";
        }

        public static class HistorialConciliacion
        {
            public const string EventoId = "eventoId";
            public const string Auditoria = "auditoria";


        }


        public static class Auditoria
        {
            public const string UsuarioProceso = "usuarioProceso";
            public const string FechaHistorial = "fechaHistorial";
            public const string Origen = "origen";


        }

        public static class HistorialEventos
        {
            public const string EventoId = "eventoId";
            public const string ParametrizacionId = "parametrizacionId";
            public const string ConciliacionId = "conciliacionId";


        }


    }
}
