namespace Conciliaciones.Aplicacion.Features.Reports.Query.ExcelDataReports
{
    /// <summary>
    /// Respuesta JSON con el archivo en base64 (workbook cifrado OLE2 si hay password).
    /// DEBUG: decodificar ArchivoBase64 a .xlsx/.bin y abrir con la password de ConciliacionExcelReportService.
    /// </summary>
    public record ExcelDataReportsResponse(
        string ArchivoBase64,
        string NombreArchivo,
        string ContentType
    );
}
