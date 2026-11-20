using Conciliaciones.Aplicacion.Features.Reports.Query.ExcelDataReports;
using Conciliaciones.Dominio.DTOs.Response.Reports;
using Conciliaciones.Dominio.Envoltorios;
using Conciliaciones.Dominio.Ports.Interfaces.Conciliations;
using Conciliaciones.Dominio.Ports.Interfaces.Reports;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using System.Net;

namespace Conciliaciones.Pruebas.Conciliaciones.Aplicacion.Features.Reports.Query.ExcelDataReports
{
    [TestClass]
    public class ExcelDataReportsHandlerTests
    {
        private readonly Mock<IConciliationExportQueryPort> _exportPort = new();
        private readonly Mock<IConciliacionExcelReportPort> _excelPort = new();

        private ExcelDataReportsHandler CreateHandler() =>
            new(_exportPort.Object, _excelPort.Object, NullLogger<ExcelDataReportsHandler>.Instance);

        [TestMethod]
        public void Constructor_NullDeps_Throws()
        {
            Assert.ThrowsExactly<ArgumentNullException>(() =>
                new ExcelDataReportsHandler(null!, _excelPort.Object, NullLogger<ExcelDataReportsHandler>.Instance));
            Assert.ThrowsExactly<ArgumentNullException>(() =>
                new ExcelDataReportsHandler(_exportPort.Object, null!, NullLogger<ExcelDataReportsHandler>.Instance));
            Assert.ThrowsExactly<ArgumentNullException>(() =>
                new ExcelDataReportsHandler(_exportPort.Object, _excelPort.Object, null!));
        }

        [TestMethod]
        public async Task Handle_NullRequest_Throws()
        {
            ExcelDataReportsHandler handler = CreateHandler();
            await Assert.ThrowsExactlyAsync<ArgumentNullException>(() =>
                handler.Handle(null!, CancellationToken.None));
        }

        [TestMethod]
        public async Task Handle_MissingParams_ReturnsBadRequest()
        {
            ExcelDataReportsHandler handler = CreateHandler();

            Answers<ExcelDataReportsResponse> emptyRegla =
                await handler.Handle(new ExcelDataReportsQuery(" "), CancellationToken.None);
            Assert.AreEqual(HttpStatusCode.BadRequest, emptyRegla.CodigoEstado);

            Answers<ExcelDataReportsResponse> emptyRegla2 =
                await handler.Handle(new ExcelDataReportsQuery(""), CancellationToken.None);
            Assert.AreEqual(HttpStatusCode.BadRequest, emptyRegla2.CodigoEstado);
        }

        [TestMethod]
        public async Task Handle_NoRows_ReturnsNotFound()
        {
            _exportPort
                .Setup(p => p.GetExportDataAsync("regla-id-1", It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ConciliacionExportDataDto(
                    "ReglaX",
                    0,
                    0m,
                    Array.Empty<ConciliacionExportColumnDto>(),
                    Array.Empty<IReadOnlyDictionary<string, object?>>()));

            Answers<ExcelDataReportsResponse> result =
                await CreateHandler().Handle(new ExcelDataReportsQuery(" regla-id-1 "), CancellationToken.None);

            Assert.AreEqual(HttpStatusCode.NotFound, result.CodigoEstado);
            _excelPort.Verify(e => e.GenerarExcelConciliaciones(It.IsAny<ConciliacionExportDataDto>()), Times.Never);
        }

        [TestMethod]
        public async Task Handle_WithRows_ReturnsBase64Ok()
        {
            ConciliacionExportDataDto data = new(
                "Regla/A:B",
                1,
                100m,
                new List<ConciliacionExportColumnDto>
                {
                    new("Monto", "monto", "currency", "banco", "monto", false)
                },
                new List<IReadOnlyDictionary<string, object?>>
                {
                    new Dictionary<string, object?> { ["monto"] = 100m }
                });

            _exportPort
                .Setup(p => p.GetExportDataAsync("regla-id-2", It.IsAny<CancellationToken>()))
                .ReturnsAsync(data);
            _excelPort
                .Setup(e => e.GenerarExcelConciliaciones(data))
                .Returns(new byte[] { 1, 2, 3, 4 });

            Answers<ExcelDataReportsResponse> result =
                await CreateHandler().Handle(new ExcelDataReportsQuery("regla-id-2"), CancellationToken.None);

            Assert.AreEqual(HttpStatusCode.OK, result.CodigoEstado);
            Assert.IsNotNull(result.Resultado);
            Assert.AreEqual(Convert.ToBase64String(new byte[] { 1, 2, 3, 4 }), result.Resultado.ArchivoBase64);
            Assert.IsTrue(result.Resultado.NombreArchivo.StartsWith("Reporte_Conciliaciones_", StringComparison.Ordinal));
            Assert.IsTrue(result.Resultado.NombreArchivo.EndsWith(".xlsx", StringComparison.Ordinal));
            Assert.AreEqual(
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                result.Resultado.ContentType);
        }
    }
}
