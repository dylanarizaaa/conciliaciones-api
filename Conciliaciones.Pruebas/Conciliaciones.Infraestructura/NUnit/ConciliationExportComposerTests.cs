using Conciliaciones.Dominio.DTOs.Response.Reports;
using Conciliaciones.Dominio.Services;
using Conciliaciones.Infraestructura.Persistence.Documents;
using Conciliaciones.Infraestructura.Persistence.Documents.Projections;
using Conciliaciones.Infraestructura.Persistence.Services;
using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using NUnit.Framework;

namespace Conciliaciones.Pruebas.Conciliaciones.Infraestructura.NUnit
{
    [TestFixture]
    public class ConciliationExportComposerTests
    {
        [Test]
        public void Compose_ExcludesAccion_IncludesEstado_AndResolvesFuenteId()
        {
            ConciliationExportComposer composer = new(
                new TransformacionMontoCalculator(),
                NullLogger<ConciliationExportComposer>.Instance);

            ConciliationExportAggregationProjection projection = new()
            {
                TotalCount = 1,
                UiConfigDoc = new ParametrizacionUiDocument
                {
                    Fuente =
                    [
                        new UiColumnDocument { Field = "banco.monto", Header = "Monto", Visible = true, Order = 1, CellConfig = new UiCellConfigDocument { Type = "currency" } },
                        new UiColumnDocument { Field = "estado", Header = "Estado", Visible = true, Order = 2 },
                        new UiColumnDocument { Field = "accion", Header = "Acción", Visible = true, Order = 3 }
                    ]
                },
                ConciliacionesFiltradas =
                [
                    new ConciliacionDocument
                    {
                        ConciliacionId = "c-1",
                        Estado = "fallido",
                        ResultadoFuentes =
                        [
                            new ResultadoFuenteDocument
                            {
                                FuenteId = "banco",
                                Data = [new Dictionary<string, object> { ["monto"] = 1500m }]
                            }
                        ],
                        ResultadoTransformacion = new Dictionary<string, object> { ["diff"] = 1500m }
                    }
                ]
            };

            ConciliacionExportDataDto data = composer.Compose(projection, "ReglaX");
            data.NombreRegla.Should().Be("ReglaX");

            data.Columns.Should().HaveCount(2);
            data.Columns.Should().NotContain(c => c.Header.Contains("Acción", StringComparison.OrdinalIgnoreCase)
                || c.RowKey.Equals("accion", StringComparison.OrdinalIgnoreCase));
            data.Columns.Should().Contain(c => c.IsEstado && c.RowKey == "estadoConciliacion");

            data.Rows.Should().ContainSingle();
            data.Rows[0]["monto"].Should().Be(1500m);
            data.Rows[0]["estadoConciliacion"].Should().Be("fallido");
            data.Rows[0].ContainsKey("accion").Should().BeFalse();
        }

        [Test]
        public void Compose_NullProjection_ReturnsEmptyDataset()
        {
            ConciliationExportComposer composer = new(
                new TransformacionMontoCalculator(),
                NullLogger<ConciliationExportComposer>.Instance);

            ConciliacionExportDataDto data = composer.Compose(null, "ReglaY");
            data.TotalCount.Should().Be(0);
            data.Columns.Should().BeEmpty();
            data.Rows.Should().BeEmpty();
            data.MontoAfectado.Should().Be(0m);
        }

        [Test]
        public void Compose_SkipsBlankFields_AndNullFuentes()
        {
            ConciliationExportComposer composer = new(
                new TransformacionMontoCalculator(),
                NullLogger<ConciliationExportComposer>.Instance);

            ConciliationExportAggregationProjection projection = new()
            {
                TotalCount = 0,
                UiConfigDoc = new ParametrizacionUiDocument
                {
                    Fuente =
                    [
                        new UiColumnDocument { Field = "   ", Header = "Blank", Visible = true, Order = 1 },
                        new UiColumnDocument { Field = "soloProp", Header = "Solo", Visible = true, Order = 2 },
                        new UiColumnDocument { Field = "banco.x", Header = "X", Visible = false, Order = 3 }
                    ]
                },
                ConciliacionesFiltradas =
                [
                    new ConciliacionDocument
                    {
                        ConciliacionId = "c-2",
                        Estado = "ok",
                        ResultadoFuentes = null
                    }
                ]
            };

            ConciliacionExportDataDto data = composer.Compose(projection, "R");
            data.Columns.Should().ContainSingle(c => c.Header == "Solo" && c.FuenteId == null);
            data.Rows.Should().ContainSingle();
            data.Rows[0]["soloProp"].Should().BeNull();
            data.TotalCount.Should().Be(1);
        }

        [Test]
        public void Compose_EmptyFuenteList_ReturnsNoColumns()
        {
            ConciliationExportComposer composer = new(
                new TransformacionMontoCalculator(),
                NullLogger<ConciliationExportComposer>.Instance);

            ConciliationExportAggregationProjection projection = new()
            {
                TotalCount = 0,
                UiConfigDoc = new ParametrizacionUiDocument { Fuente = [] },
                ConciliacionesFiltradas = null
            };

            ConciliacionExportDataDto data = composer.Compose(projection, "r");
            data.Columns.Should().BeEmpty();
            data.Rows.Should().BeEmpty();
        }

        [Test]
        public void Constructor_NullDeps_Throws()
        {
            Action a1 = () => _ = new ConciliationExportComposer(null!, NullLogger<ConciliationExportComposer>.Instance);
            Action a2 = () => _ = new ConciliationExportComposer(new TransformacionMontoCalculator(), null!);
            a1.Should().Throw<ArgumentNullException>();
            a2.Should().Throw<ArgumentNullException>();
        }
    }
}
