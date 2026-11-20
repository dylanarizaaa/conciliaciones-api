using Conciliaciones.Dominio.DTOs.Response.Conciliation;
using Conciliaciones.Infraestructura.Persistence.Documents;
using Conciliaciones.Infraestructura.Persistence.Documents.Projections;
using Conciliaciones.Infraestructura.Ports.Interfaces.Persistence;

namespace Conciliaciones.Infraestructura.Persistence.Services
{
    public sealed class ConciliationFuentesDetailComposer : IConciliationFuentesDetailComposer
    {
        public List<FuenteDetalleDto> Compose(ConciliationFuentesDetailProjection? docResult)
        {
            if (docResult == null)
            {
                return new List<FuenteDetalleDto>();
            }

            string nombreProceso = string.IsNullOrWhiteSpace(docResult.NombreProceso) ? "Proceso" : docResult.NombreProceso;
            Dictionary<string, List<UiColumnDocument>> uiDatosMap = BuildUiDatosMap(docResult.UiConfigDoc);
            List<FuenteDetalleDto> fuentesList = new List<FuenteDetalleDto>();

            if (docResult.ResultadoFuentes == null)
            {
                return fuentesList;
            }

            foreach (ResultadoFuenteDocument fuenteDoc in docResult.ResultadoFuentes)
            {
                fuentesList.Add(MapFuente(fuenteDoc, nombreProceso, uiDatosMap));
            }

            return fuentesList;
        }

        private static Dictionary<string, List<UiColumnDocument>> BuildUiDatosMap(ParametrizacionUiDocument? uiConfig)
        {
            Dictionary<string, List<UiColumnDocument>> uiDatosMap = new Dictionary<string, List<UiColumnDocument>>();
            if (uiConfig?.Datos == null)
            {
                return uiDatosMap;
            }

            foreach (UiDatosBlockDocument datoDoc in uiConfig.Datos)
            {
                uiDatosMap[datoDoc.IdFuente] = datoDoc.Columns ?? new List<UiColumnDocument>();
            }

            return uiDatosMap;
        }

        private static FuenteDetalleDto MapFuente(
            ResultadoFuenteDocument fuenteDoc,
            string nombreProceso,
            Dictionary<string, List<UiColumnDocument>> uiDatosMap)
        {
            string fuenteId = fuenteDoc.FuenteId ?? string.Empty;
            string conexionId = fuenteDoc.ConexionId ?? string.Empty;
            string nombreConcatenado = string.IsNullOrWhiteSpace(conexionId)
                ? nombreProceso
                : $"{nombreProceso} {conexionId}";

            Dictionary<string, object>? dataFirstDoc = fuenteDoc.Data != null && fuenteDoc.Data.Count > 0
                ? fuenteDoc.Data[0]
                : null;

            List<DatoDetalleDto> datosDetails = new List<DatoDetalleDto>();
            if (uiDatosMap.TryGetValue(fuenteId, out List<UiColumnDocument>? uiColumns))
            {
                datosDetails.AddRange(uiColumns.Select(col => MapDato(col, dataFirstDoc)));
            }

            return new FuenteDetalleDto(nombreConcatenado, datosDetails);
        }

        private static DatoDetalleDto MapDato(UiColumnDocument colConfig, Dictionary<string, object>? dataFirstDoc)
        {
            string fieldName = colConfig.Field;
            string headerName = string.IsNullOrWhiteSpace(colConfig.Header) ? fieldName : colConfig.Header;
            string typeName = colConfig.CellConfig?.Type ?? colConfig.Cell?.Type ?? "text";

            object? valorFinal = null;
            if (dataFirstDoc != null
                && dataFirstDoc.TryGetValue(fieldName, out object? raw)
                && raw != null)
            {
                valorFinal = raw;
            }

            return new DatoDetalleDto(headerName, typeName, valorFinal);
        }
    }
}
