using Conciliaciones.Dominio.DTOs.Response.Reports;
using Conciliaciones.Infraestructura.Reports;
using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using NUnit.Framework;

namespace Conciliaciones.Pruebas.Conciliaciones.Infraestructura.NUnit
{
    [TestFixture]
    public class ConciliacionExcelReportServiceTests
    {
        private string _passwordBackup = null!;

        [SetUp]
        public void SetUp()
        {
            _passwordBackup = ConciliacionExcelReportService.ExcelWorkbookPassword;
        }

        [TearDown]
        public void TearDown()
        {
            ConciliacionExcelReportService.ExcelWorkbookPassword = _passwordBackup;
        }

        [Test]
        public void Constructor_NullLogger_Throws()
        {
            Action act = () => _ = new ConciliacionExcelReportService(null!);
            act.Should().Throw<ArgumentNullException>();
        }

        [Test]
        public void GenerarExcel_NullData_Throws()
        {
            ConciliacionExcelReportService svc = new(NullLogger<ConciliacionExcelReportService>.Instance);
            Action act = () => svc.GenerarExcelConciliaciones(null!);
            act.Should().Throw<ArgumentNullException>();
        }

        [Test]
        public void GenerarExcel_WithPassword_ReturnsEncryptedBytes()
        {
            ConciliacionExcelReportService.ExcelWorkbookPassword = "1234";
            ConciliacionExcelReportService svc = new(NullLogger<ConciliacionExcelReportService>.Instance);

            ConciliacionExportDataDto data = BuildSampleData();
            byte[] bytes = svc.GenerarExcelConciliaciones(data);

            bytes.Should().NotBeEmpty();
            // OLE2 magic / compound file signature starts with D0 CF 11 E0
            bytes[0].Should().Be(0xD0);
            bytes[1].Should().Be(0xCF);
        }

        [Test]
        public void GenerarExcel_WithoutPassword_ReturnsPlainXlsx()
        {
            ConciliacionExcelReportService.ExcelWorkbookPassword = string.Empty;
            ConciliacionExcelReportService svc = new(NullLogger<ConciliacionExcelReportService>.Instance);

            ConciliacionExportDataDto data = BuildSampleData();
            byte[] bytes = svc.GenerarExcelConciliaciones(data);

            bytes.Should().NotBeEmpty();
            // ZIP/xlsx signature PK
            bytes[0].Should().Be((byte)'P');
            bytes[1].Should().Be((byte)'K');
        }

        [Test]
        public void GenerarExcel_EmptyColumns_StillBuildsWorkbook()
        {
            ConciliacionExcelReportService.ExcelWorkbookPassword = string.Empty;
            ConciliacionExcelReportService svc = new(NullLogger<ConciliacionExcelReportService>.Instance);

            ConciliacionExportDataDto data = new(
                "regla",
                1,
                0m,
                Array.Empty<ConciliacionExportColumnDto>(),
                new List<IReadOnlyDictionary<string, object?>>
                {
                    new Dictionary<string, object?>()
                });

            byte[] bytes = svc.GenerarExcelConciliaciones(data);
            bytes.Should().NotBeEmpty();
        }

        [Test]
        public void GenerarExcel_CoversNumericAndDateCellPaths()
        {
            ConciliacionExcelReportService.ExcelWorkbookPassword = string.Empty;
            ConciliacionExcelReportService svc = new(NullLogger<ConciliacionExcelReportService>.Instance);

            List<ConciliacionExportColumnDto> columns =
            [
                new("D", "d", null, null, "d", false),
                new("F", "f", null, null, "f", false),
                new("M", "m", null, null, "m", false),
                new("I", "i", "currency", null, "i", false),
                new("L", "l", "currency", null, "l", false),
                new("S", "s", "currency", null, "s", false),
                new("N", "n", null, null, "n", false),
                new("T", "t", "text", null, "t", false),
                new("X", "x", null, null, "x", false)
            ];

            Dictionary<string, object?> row = new()
            {
                ["d"] = 1.5d,
                ["f"] = 2.5f,
                ["m"] = 3.5m,
                ["i"] = 10,
                ["l"] = 20L,
                ["s"] = "99.5",
                ["n"] = null,
                ["t"] = new DateTime(2024, 6, 15, 10, 30, 0),
                ["x"] = "texto"
            };

            ConciliacionExportDataDto data = new(
                "regla",
                1,
                50m,
                columns,
                new List<IReadOnlyDictionary<string, object?>> { row });

            byte[] bytes = svc.GenerarExcelConciliaciones(data);
            bytes.Should().NotBeEmpty();
        }

        private static ConciliacionExportDataDto BuildSampleData() =>
            new(
                "ReglaX",
                2,
                2500m,
                new List<ConciliacionExportColumnDto>
                {
                    new("Estado", "estadoConciliacion", null, null, null, true),
                    new("Monto", "monto", "currency", "banco", "monto", false),
                    new("Ref", "ref", "text", "banco", "ref", false)
                },
                new List<IReadOnlyDictionary<string, object?>>
                {
                    new Dictionary<string, object?>
                    {
                        ["estadoConciliacion"] = "fallido",
                        ["monto"] = 1500m,
                        ["ref"] = "A-1"
                    },
                    new Dictionary<string, object?>
                    {
                        ["estadoConciliacion"] = "fallido",
                        ["monto"] = 1000d,
                        ["ref"] = null
                    }
                });
    }
}
