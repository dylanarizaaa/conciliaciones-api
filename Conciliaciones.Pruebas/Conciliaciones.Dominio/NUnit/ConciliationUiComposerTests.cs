using Conciliaciones.Dominio.DTOs.Response.Conciliation;
using Conciliaciones.Dominio.Services;
using FluentAssertions;
using NUnit.Framework;

namespace Conciliaciones.Pruebas.Conciliaciones.Dominio.NUnit
{
    [TestFixture]
    public class ConciliationUiComposerTests
    {
        private ConciliationUiComposer _sut = null!;

        [SetUp]
        public void SetUp() => _sut = new ConciliationUiComposer();

        [Test]
        public void ComposeDiscrepancyTable_HappyPath_OrdersVisibleColumns()
        {
            // Arrange
            List<UiColumnSpec> columns = new()
            {
                new("b", "B", true, true, 2, "text", null, null),
                new("a", "A", true, true, 1, "currency", null, null),
                new("hidden", "H", false, false, 0, "text", null, null)
            };

            // Act
            ColumsDetailsDto result = _sut.ComposeDiscrepancyTable(columns);

            // Assert
            result.Blocks.Should().HaveCount(1);
            result.Blocks[0].Config!.Columns.Select(c => c.Field).Should().Equal("a", "b");
        }

        [Test]
        public void ComposeDiscrepancyTable_NullColumns_ReturnsEmptyTable()
        {
            ColumsDetailsDto result = _sut.ComposeDiscrepancyTable(null!);

            result.Blocks.Should().HaveCount(1);
            result.Blocks[0].Config!.Columns.Should().BeEmpty();
        }

        [Test]
        public void ComposeDiscrepancyTable_EmptyColumns_ReturnsEmptyTable()
        {
            ColumsDetailsDto result = _sut.ComposeDiscrepancyTable(Array.Empty<UiColumnSpec>());

            result.Blocks[0].Config!.Columns.Should().BeEmpty();
        }

        [Test]
        public void MapColumn_WithVariantsAndConfig_MapsCellConfig()
        {
            UiColumnSpec spec = new(
                "estado",
                "Estado",
                true,
                true,
                1,
                "badge",
                new Dictionary<string, string> { ["Pendiente"] = "destructive" },
                new Dictionary<string, object> { ["x"] = 1 });

            ColumnUiDto column = _sut.MapColumn(spec);

            column.Cell.Should().NotBeNull();
            column.Cell!.Type.Should().Be("badge");
            column.Cell.VariantByValue.Should().ContainKey("Pendiente");
            column.Cell.Config.Should().ContainKey("x");
        }

        [Test]
        public void MapColumn_WithoutCellMetadata_CellIsNull()
        {
            UiColumnSpec spec = new("f", "H", false, true, 1, null, null, null);

            _sut.MapColumn(spec).Cell.Should().BeNull();
        }
    }
}
