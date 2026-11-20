using Conciliaciones.Dominio.DTOs.Response.Reports;
using Conciliaciones.Infraestructura.Persistence.DocumentDB;
using Conciliaciones.Infraestructura.Persistence.Documents;
using Conciliaciones.Infraestructura.Persistence.Documents.Projections;
using Conciliaciones.Infraestructura.Persistence.Repositories;
using Conciliaciones.Infraestructura.Ports.Interfaces.Persistence;
using Conciliaciones.Infraestructura.Settings;
using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using Moq;
using NUnit.Framework;

namespace Conciliaciones.Pruebas.Conciliaciones.Infraestructura.NUnit
{
    [TestFixture]
    public class ConciliationExportRepositoryTests
    {
        [Test]
        public void Constructor_NullDeps_Throws()
        {
            Mock<DocumentDBContext> ctx = CreateContext();
            Mock<IConciliationExportComposer> composer = new();

            Action a1 = () => _ = new ConciliationExportRepository(null!, composer.Object, NullLogger<ConciliationExportRepository>.Instance);
            Action a2 = () => _ = new ConciliationExportRepository(ctx.Object, null!, NullLogger<ConciliationExportRepository>.Instance);
            Action a3 = () => _ = new ConciliationExportRepository(ctx.Object, composer.Object, null!);

            a1.Should().Throw<ArgumentNullException>();
            a2.Should().Throw<ArgumentNullException>();
            a3.Should().Throw<ArgumentNullException>();
        }

        [Test]
        public async Task GetExportDataAsync_WhenProjectionFound_ComposesResult()
        {
            ConciliationExportAggregationProjection projection = new()
            {
                TotalCount = 1,
                UiConfigDoc = new ParametrizacionUiDocument(),
                ConciliacionesFiltradas = []
            };

            ConciliacionExportDataDto expected = new(
                "ReglaX",
                1,
                10m,
                Array.Empty<ConciliacionExportColumnDto>(),
                Array.Empty<IReadOnlyDictionary<string, object?>>());

            Mock<IAsyncCursor<ConciliationExportAggregationProjection>> cursor = CreateCursor(new List<ConciliationExportAggregationProjection> { projection });
            Mock<IMongoCollection<ParametrizacionDocument>> collection = new();
            collection.Setup(c => c.AggregateAsync(
                    It.IsAny<PipelineDefinition<ParametrizacionDocument, ConciliationExportAggregationProjection>>(),
                    It.IsAny<AggregateOptions>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(cursor.Object);

            Mock<DocumentDBContext> ctx = CreateContext();
            ctx.Setup(c => c.GetCollection<ParametrizacionDocument>(It.IsAny<string>())).Returns(collection.Object);

            Mock<IConciliationExportComposer> composer = new();
            composer.Setup(c => c.Compose(projection, "ReglaX")).Returns(expected);

            ConciliationExportRepository repo = new(
                ctx.Object,
                composer.Object,
                NullLogger<ConciliationExportRepository>.Instance);

            ConciliacionExportDataDto result = await repo.GetExportDataAsync("ReglaX", CancellationToken.None);
            result.Should().BeSameAs(expected);
            composer.Verify(c => c.Compose(projection, "ReglaX"), Times.Once);
        }

        [Test]
        public async Task GetExportDataAsync_WhenEmptyCursor_PassesNullProjection()
        {
            Mock<IAsyncCursor<ConciliationExportAggregationProjection>> cursor =
                CreateCursor(new List<ConciliationExportAggregationProjection>());
            Mock<IMongoCollection<ParametrizacionDocument>> collection = new();
            collection.Setup(c => c.AggregateAsync(
                    It.IsAny<PipelineDefinition<ParametrizacionDocument, ConciliationExportAggregationProjection>>(),
                    It.IsAny<AggregateOptions>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(cursor.Object);

            Mock<DocumentDBContext> ctx = CreateContext();
            ctx.Setup(c => c.GetCollection<ParametrizacionDocument>(It.IsAny<string>())).Returns(collection.Object);

            ConciliacionExportDataDto empty = new(
                "r",
                0,
                0m,
                Array.Empty<ConciliacionExportColumnDto>(),
                Array.Empty<IReadOnlyDictionary<string, object?>>());

            Mock<IConciliationExportComposer> composer = new();
            composer.Setup(c => c.Compose(null, "r")).Returns(empty);

            ConciliationExportRepository repo = new(
                ctx.Object,
                composer.Object,
                NullLogger<ConciliationExportRepository>.Instance);

            ConciliacionExportDataDto result = await repo.GetExportDataAsync("r", CancellationToken.None);
            result.TotalCount.Should().Be(0);
            composer.Verify(c => c.Compose(null, "r"), Times.Once);
        }

        private static Mock<IAsyncCursor<T>> CreateCursor<T>(List<T> items)
        {
            Mock<IAsyncCursor<T>> cursor = new();
            if (items.Count == 0)
            {
                cursor.SetupSequence(c => c.MoveNextAsync(It.IsAny<CancellationToken>()))
                    .ReturnsAsync(false);
                cursor.Setup(c => c.Current).Returns(Array.Empty<T>());
            }
            else
            {
                cursor.SetupSequence(c => c.MoveNextAsync(It.IsAny<CancellationToken>()))
                    .ReturnsAsync(true)
                    .ReturnsAsync(false);
                cursor.Setup(c => c.Current).Returns(items);
            }

            return cursor;
        }

        private static Mock<DocumentDBContext> CreateContext()
        {
            IOptions<DocumentDbSettings> options = Options.Create(new DocumentDbSettings
            {
                ConnectionString = "mongodb://localhost:27017",
                DatabaseName = "ConciliacionesTestDb",
                UseSsl = false
            });
            return new Mock<DocumentDBContext>(options);
        }
    }
}
