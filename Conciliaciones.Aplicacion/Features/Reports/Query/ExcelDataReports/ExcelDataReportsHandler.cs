using Conciliaciones.Dominio.DTOs.Response.Reports;
using Conciliaciones.Dominio.Envoltorios;
using Conciliaciones.Dominio.Ports.Interfaces.Conciliations;
using Conciliaciones.Dominio.Ports.Interfaces.Reports;
using MediatR;
using Microsoft.Extensions.Logging;
using System.Net;

namespace Conciliaciones.Aplicacion.Features.Reports.Query.ExcelDataReports
{
    public sealed class ExcelDataReportsHandler : IRequestHandler<ExcelDataReportsQuery, Answers<ExcelDataReportsResponse>>
    {
        private readonly IConciliationExportQueryPort _exportQueryPort;
        private readonly IConciliacionExcelReportPort _excelReportPort;
        private readonly ILogger<ExcelDataReportsHandler> _logger;

        public ExcelDataReportsHandler(
            IConciliationExportQueryPort exportQueryPort,
            IConciliacionExcelReportPort excelReportPort,
            ILogger<ExcelDataReportsHandler> logger)
        {
            _exportQueryPort = exportQueryPort ?? throw new ArgumentNullException(nameof(exportQueryPort));
            _excelReportPort = excelReportPort ?? throw new ArgumentNullException(nameof(excelReportPort));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<Answers<ExcelDataReportsResponse>> Handle(ExcelDataReportsQuery request, CancellationToken cancellationToken)
        {
            if (request == null)
            {
                throw new ArgumentNullException(nameof(request));
            }

            if (string.IsNullOrWhiteSpace(request.ReglaId))
            {
                return new Answers<ExcelDataReportsResponse>("reglaId es obligatorio.", HttpStatusCode.BadRequest);
            }

            string reglaId = request.ReglaId.Trim();

            _logger.LogInformation(
                "[ExportExcel][Handler] Inicio. reglaId={ReglaId}",
                reglaId);

            ConciliacionExportDataDto exportData = await _exportQueryPort.GetExportDataAsync(
                reglaId,
                cancellationToken);

            if (exportData.TotalCount <= 0 || exportData.Rows.Count == 0)
            {
                _logger.LogWarning(
                    "[ExportExcel][Handler] Sin filas. reglaId={ReglaId} totalCount={TotalCount} columns={Columns}",
                    reglaId,
                    exportData.TotalCount,
                    exportData.Columns.Count);
                return new Answers<ExcelDataReportsResponse>("No hay conciliaciones para exportar con los filtros indicados.", HttpStatusCode.NotFound);
            }

            byte[] fileBytes = _excelReportPort.GenerarExcelConciliaciones(exportData);
            string base64 = Convert.ToBase64String(fileBytes);

            _logger.LogInformation(
                "[ExportExcel][Handler] OK. bytes={ByteLength} base64Length={Base64Length} casos={Casos}",
                fileBytes.Length,
                base64.Length,
                exportData.TotalCount);

            string fileLabel = string.IsNullOrWhiteSpace(exportData.NombreRegla) ? reglaId : exportData.NombreRegla;
            string safeRegla = string.Join("_", fileLabel.Split(Path.GetInvalidFileNameChars()));
            ExcelDataReportsResponse response = new(
                ArchivoBase64: base64,
                NombreArchivo: $"Reporte_Conciliaciones_{safeRegla}.xlsx",
                ContentType: "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet");

            return new Answers<ExcelDataReportsResponse>(response);
        }
    }
}
