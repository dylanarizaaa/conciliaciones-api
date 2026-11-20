using Conciliaciones.Dominio.DTOs.Requests.Conciliation;
using Conciliaciones.Dominio.DTOs.Response.Conciliation;
using Conciliaciones.Dominio.Ports.Interfaces.Conciliations;
using Conciliaciones.Dominio.Services;
using Conciliaciones.Infraestructura.Persistence.DocumentDB;
using Conciliaciones.Infraestructura.Persistence.Documents;
using Conciliaciones.Infraestructura.Persistence.Documents.Projections;
using Conciliaciones.Infraestructura.Persistence.Hierarchy;
using Conciliaciones.Infraestructura.Persistence.Repositories;
using Conciliaciones.Infraestructura.Persistence.Services;
using Conciliaciones.Infraestructura.Ports.Interfaces.Persistence;
using Conciliaciones.Infraestructura.Settings;
using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Driver;
using Moq;

namespace Conciliaciones.Pruebas.Conciliaciones.Infraestructura.Persistence.Repositories
{
    [TestClass]
    public class ConciliationsRepositoryTests
    {
        [TestMethod]
        public async Task GetConciliationByIdSp_ConDatosValidos_DevuelveColumnsDetailsDto()
        {
            ConciliationUiAggregationProjection projection = new ConciliationUiAggregationProjection
            {
                UiConfigDoc = new ParametrizacionUiDocument
                {
                    Fuente = new List<UiColumnDocument>
                    {
                        new UiColumnDocument
                        {
                            Field = "fuenteFacturas.FacturaId",
                            Header = "Factura ID",
                            Sortable = true,
                            Visible = true,
                            Order = 1
                        }
                    }
                }
            };

            ConciliationsRepository repository = CreateQueryRepository(projection, null, null);
            ColumsDetailsDto result = await repository.GetConciliationByIdSp(
                "ValidacionMontoExacto",
                CancellationToken.None);

            Assert.IsNotNull(result);
            Assert.HasCount(1, result.Blocks);
            Assert.AreEqual("discrepancias-table", result.Blocks[0].Id);
            Assert.AreEqual("Diferencias detectadas", result.Blocks[0].Title);
            Assert.AreEqual("fuenteFacturas.FacturaId", result.Blocks[0].Config.Columns[0].Field);
            Assert.AreEqual("Factura ID", result.Blocks[0].Config.Columns[0].Header);
        }

        [TestMethod]
        public async Task GetConciliationRowsAsync_ConPaginacionValida_DevuelvePagedRowsDto()
        {
            ConciliationRowsAggregationProjection projection = new ConciliationRowsAggregationProjection
            {
                TotalCount = 1,
                ConciliacionesPaginadas = new List<ConciliacionDocument>
                {
                    new ConciliacionDocument
                    {
                        ConciliacionId = "CONC-001",
                        Estado = "fallido",
                        ResultadoTransformacion = new Dictionary<string, object> { { "diferencia", 1500.50m } }
                    }
                },
                UiConfigDoc = new ParametrizacionUiDocument
                {
                    Fuente = new List<UiColumnDocument>
                    {
                        new UiColumnDocument
                        {
                            Field = "fuenteFacturas.FacturaId",
                            Visible = true,
                            Order = 1
                        }
                    }
                }
            };

            ConciliationsRepository repository = CreateQueryRepository(null, projection, null);
            PagedRowsDto result = await repository.GetConciliationRowsAsync("ReglaTest", 1, 10, CancellationToken.None);

            Assert.IsNotNull(result);
            Assert.AreEqual(1, result.CasosTotales);
            Assert.AreEqual(1500.50m, result.MontoAfectado);
            Assert.AreEqual(1, result.TotalCount);
        }

        [TestMethod]
        public async Task UpdateEstadoConciliacionAsync_ConListaDeGuids_CreaEventoYEliminaConciliacion()
        {
            Guid guid1 = Guid.NewGuid();
            Guid guid2 = Guid.NewGuid();

            List<ConciliacionDocument> findResult =
            [
                new ConciliacionDocument
                {
                    ConciliacionId = guid1.ToString(),
                    ParametrizacionId = "param-1",
                    ParametrosEntrada = new Dictionary<string, object> { ["codigoProductoParm"] = "700012140219" }
                }
            ];

            Mock<IAsyncCursor<ConciliacionDocument>> findCursorMock = CreateCursor(findResult);

            Mock<IMongoCollection<ConciliacionDocument>> collectionMock = new Mock<IMongoCollection<ConciliacionDocument>>();
            collectionMock.Setup(c => c.FindAsync(
                It.IsAny<FilterDefinition<ConciliacionDocument>>(),
                It.IsAny<FindOptions<ConciliacionDocument, ConciliacionDocument>>(),
                It.IsAny<CancellationToken>()
            )).ReturnsAsync(findCursorMock.Object);

            collectionMock.Setup(c => c.UpdateManyAsync(
                It.IsAny<FilterDefinition<ConciliacionDocument>>(),
                It.IsAny<UpdateDefinition<ConciliacionDocument>>(),
                It.IsAny<UpdateOptions?>(),
                It.IsAny<CancellationToken>()
            )).ReturnsAsync(new UpdateResult.Acknowledged(1, 1, null));

            Mock<IMongoCollection<EventoDocument>> eventosMock = new Mock<IMongoCollection<EventoDocument>>();
            eventosMock.Setup(c => c.InsertManyAsync(
                It.IsAny<IEnumerable<EventoDocument>>(),
                It.IsAny<InsertManyOptions>(),
                It.IsAny<CancellationToken>()
            )).Returns(Task.CompletedTask);

            Mock<IMongoCollection<BsonDocument>> historialLecturaMock = new Mock<IMongoCollection<BsonDocument>>();
            Mock<IAsyncCursor<BsonDocument>> historialCursor = CreateCursor(
            [
                new BsonDocument
                {
                    { "conciliacionId", guid1.ToString() },
                    { "eventoId", "evento-antiguo" },
                    { "auditoria", new BsonDocument("fechaHistorial", new DateTime(2026, 11, 17, 16, 5, 11, DateTimeKind.Utc)) }
                },
                new BsonDocument
                {
                    { "conciliacionId", guid1.ToString() },
                    { "eventoId", "2740d9f5-406b-4a5b-b4d6-0225f0206112" },
                    { "auditoria", new BsonDocument("fechaHistorial", new DateTime(2026, 11, 17, 20, 30, 24, DateTimeKind.Utc)) }
                }
            ]);
            historialLecturaMock.Setup(c => c.FindAsync(
                It.IsAny<FilterDefinition<BsonDocument>>(),
                It.IsAny<FindOptions<BsonDocument, BsonDocument>>(),
                It.IsAny<CancellationToken>()
            )).ReturnsAsync(historialCursor.Object);

            Mock<IMongoCollection<HistorialConciliacionesDocument>> historialMock = new Mock<IMongoCollection<HistorialConciliacionesDocument>>();
            historialMock.Setup(c => c.InsertManyAsync(
                It.IsAny<IEnumerable<HistorialConciliacionesDocument>>(),
                It.IsAny<InsertManyOptions>(),
                It.IsAny<CancellationToken>()
            )).Returns(Task.CompletedTask);

            Mock<DocumentDBContext> contextMock = CreateContext();
            contextMock.Setup(c => c.GetCollection<ConciliacionDocument>(It.IsAny<string>())).Returns(collectionMock.Object);
            contextMock.Setup(c => c.GetCollection<EventoDocument>(It.IsAny<string>())).Returns(eventosMock.Object);
            contextMock.Setup(c => c.GetCollection<HistorialConciliacionesDocument>(It.IsAny<string>())).Returns(historialMock.Object);
            contextMock.Setup(c => c.GetCollection<BsonDocument>(It.IsAny<string>())).Returns(historialLecturaMock.Object);
            ConciliationsRepository repository = CreateFacade(contextMock.Object);

            StatusUpdateConciliation result = await repository.UpdateEstadoConciliacionAsync(
                new List<Guid> { guid1, guid2 },
                CancellationToken.None);

            Assert.IsNotNull(result);
            Assert.HasCount(1, result.Actualizados);
            Assert.HasCount(1, result.NoEncontrados);
            Assert.AreEqual(2, result.TotalProcesados);
            Assert.AreEqual(guid1, result.Actualizados[0]);
            Assert.AreEqual(guid2, result.NoEncontrados[0]);

            eventosMock.Verify(c => c.InsertManyAsync(
                It.Is<IEnumerable<EventoDocument>>(docs =>
                    docs.Count() == 1
                    && docs.First().ConciliacionId == guid1.ToString()
                    && docs.First().ParametrizacionId == "param-1"),
                It.IsAny<InsertManyOptions>(),
                It.IsAny<CancellationToken>()), Times.Once);
            historialMock.Verify(c => c.InsertManyAsync(
                It.Is<IEnumerable<HistorialConciliacionesDocument>>(docs =>
                    docs.Count() == 1
                    && docs.First().Estado == "VALIDANDO"
                    && docs.First().Auditoria != null
                    && docs.First().Auditoria!.FechaHistorial.HasValue
                    && docs.First().EventoId == "2740d9f5-406b-4a5b-b4d6-0225f0206112"),
                It.IsAny<InsertManyOptions>(),
                It.IsAny<CancellationToken>()), Times.Once);
            collectionMock.Verify(c => c.UpdateManyAsync(
                It.IsAny<FilterDefinition<ConciliacionDocument>>(),
                It.IsAny<UpdateDefinition<ConciliacionDocument>>(),
                It.IsAny<UpdateOptions?>(),
                It.IsAny<CancellationToken>()), Times.Once);
            collectionMock.Verify(c => c.DeleteManyAsync(
                It.IsAny<FilterDefinition<ConciliacionDocument>>(),
                It.IsAny<CancellationToken>()), Times.Never);
        }

        [TestMethod]
        public async Task UpdateEstadoConciliacionAsync_SinHistorialPrevio_UsaElEventoNuevo()
        {
            Guid conciliacionId = Guid.NewGuid();
            (Mock<IMongoCollection<HistorialConciliacionesDocument>> historialMock, _) =
                await EjecutarCambioEstadoAsync(conciliacionId, new List<BsonDocument>());

            historialMock.Verify(c => c.InsertManyAsync(
                It.Is<IEnumerable<HistorialConciliacionesDocument>>(docs =>
                    docs.Count() == 1
                    && docs.First().EventoId != null
                    && docs.First().EventoId.Length == 36),
                It.IsAny<InsertManyOptions>(),
                It.IsAny<CancellationToken>()), Times.Once);
        }

        [TestMethod]
        public async Task UpdateEstadoConciliacionAsync_HistorialIncompleto_TomaElUltimoEventoValidoPorFecha()
        {
            Guid conciliacionId = Guid.NewGuid();
            List<BsonDocument?> documentos =
            [
                null,
                new BsonDocument { { "conciliacionId", conciliacionId.ToString() } },
                new BsonDocument
                {
                    { "conciliacionId", conciliacionId.ToString() },
                    { "eventoId", "   " }
                },
                new BsonDocument
                {
                    { "conciliacionId", 10 },
                    { "eventoId", "ignorado" }
                },
                new BsonDocument
                {
                    { "conciliacionId", conciliacionId.ToString() },
                    { "eventoId", "por-fecha-creacion" },
                    { "auditoria", "texto" },
                    { "fechaCreacion", new DateTime(2026, 11, 17, 16, 0, 0, DateTimeKind.Utc) }
                },
                new BsonDocument
                {
                    { "conciliacionId", conciliacionId.ToString() },
                    { "eventoId", "sin-fecha" }
                },
                new BsonDocument
                {
                    { "conciliacionId", conciliacionId.ToString() },
                    { "eventoId", "2740d9f5-406b-4a5b-b4d6-0225f0206112" },
                    { "auditoria", new BsonDocument("fechaHistorial", new DateTime(2026, 11, 17, 20, 30, 24, DateTimeKind.Utc)) }
                }
            ];

            (Mock<IMongoCollection<HistorialConciliacionesDocument>> historialMock, _) =
                await EjecutarCambioEstadoAsync(conciliacionId, documentos!);

            historialMock.Verify(c => c.InsertManyAsync(
                It.Is<IEnumerable<HistorialConciliacionesDocument>>(docs =>
                    docs.Single().EventoId == "2740d9f5-406b-4a5b-b4d6-0225f0206112"),
                It.IsAny<InsertManyOptions>(),
                It.IsAny<CancellationToken>()), Times.Once);
        }

        [TestMethod]
        public async Task UpdateEstadoConciliacionAsync_CursorDeHistorialNulo_UsaElEventoNuevo()
        {
            Guid conciliacionId = Guid.NewGuid();
            (Mock<IMongoCollection<HistorialConciliacionesDocument>> historialMock, _) =
                await EjecutarCambioEstadoAsync(conciliacionId, null);

            historialMock.Verify(c => c.InsertManyAsync(
                It.Is<IEnumerable<HistorialConciliacionesDocument>>(docs =>
                    docs.Count() == 1 && !string.IsNullOrWhiteSpace(docs.First().EventoId)),
                It.IsAny<InsertManyOptions>(),
                It.IsAny<CancellationToken>()), Times.Once);
        }

        [TestMethod]
        public async Task UpdateEstadoConciliacionAsync_SinIdsEncontrados_NoCreaEventoNiElimina()
        {
            Guid missing = Guid.NewGuid();
            Mock<IAsyncCursor<ConciliacionDocument>> findCursorMock = CreateCursor(new List<ConciliacionDocument>());

            Mock<IMongoCollection<ConciliacionDocument>> collectionMock = new Mock<IMongoCollection<ConciliacionDocument>>();
            collectionMock.Setup(c => c.FindAsync(
                It.IsAny<FilterDefinition<ConciliacionDocument>>(),
                It.IsAny<FindOptions<ConciliacionDocument, ConciliacionDocument>>(),
                It.IsAny<CancellationToken>()
            )).ReturnsAsync(findCursorMock.Object);

            Mock<IMongoCollection<EventoDocument>> eventosMock = new Mock<IMongoCollection<EventoDocument>>();

            Mock<DocumentDBContext> contextMock = CreateContext();
            contextMock.Setup(c => c.GetCollection<ConciliacionDocument>(It.IsAny<string>())).Returns(collectionMock.Object);
            contextMock.Setup(c => c.GetCollection<EventoDocument>(It.IsAny<string>())).Returns(eventosMock.Object);
            ConciliationsRepository repository = CreateFacade(contextMock.Object);

            StatusUpdateConciliation result = await repository.UpdateEstadoConciliacionAsync(
                new List<Guid> { missing },
                CancellationToken.None);

            Assert.IsEmpty(result.Actualizados);
            Assert.HasCount(1, result.NoEncontrados);
            eventosMock.Verify(c => c.InsertManyAsync(
                It.IsAny<IEnumerable<EventoDocument>>(),
                It.IsAny<InsertManyOptions>(),
                It.IsAny<CancellationToken>()), Times.Never);
            collectionMock.Verify(c => c.DeleteManyAsync(
                It.IsAny<FilterDefinition<ConciliacionDocument>>(),
                It.IsAny<CancellationToken>()), Times.Never);
        }

        [TestMethod]
        public async Task CreateParametrizacionAsync_DelegaAlWritePort()
        {
            Mock<IConciliationQueryPort> query = new();
            Mock<IConciliationStatePort> state = new();
            Mock<IParametrizacionWritePort> write = new();
            CreateParametrizacionRequest request = new() { proyecto = "p", proceso = "pr", subproceso = "sp" };
            CreateParametrizacionResponse response = new("proy-1", "proc-1", "sub-1", "param-1", "ok");
            write.Setup(w => w.CreateParametrizacionAsync(request, It.IsAny<CancellationToken>()))
                .ReturnsAsync(response);

            ConciliationsRepository repository = new(query.Object, state.Object, write.Object);
            CreateParametrizacionResponse result = await repository.CreateParametrizacionAsync(request, CancellationToken.None);

            Assert.AreEqual("param-1", result.ParametrizacionId);
            write.Verify(w => w.CreateParametrizacionAsync(request, It.IsAny<CancellationToken>()), Times.Once);
        }

        [TestMethod]
        public async Task GetConciliationFuentesDetailAsync_ConConciliacionId_DevuelveListaFuentes()
        {
            ConciliationFuentesDetailProjection projection = new ConciliationFuentesDetailProjection
            {
                NombreProceso = "ProcesoFacturacion",
                UiConfigDoc = new ParametrizacionUiDocument
                {
                    Datos = new List<UiDatosBlockDocument>
                    {
                        new UiDatosBlockDocument
                        {
                            IdFuente = "fuente1",
                            Columns = new List<UiColumnDocument>
                            {
                                new UiColumnDocument
                                {
                                    Field = "Monto",
                                    Header = "Monto Total",
                                    CellConfig = new UiCellConfigDocument { Type = "currency" }
                                }
                            }
                        }
                    }
                },
                ResultadoFuentes = new List<ResultadoFuenteDocument>
                {
                    new ResultadoFuenteDocument
                    {
                        FuenteId = "fuente1",
                        ConexionId = "Conn1",
                        Data = new List<Dictionary<string, object>>
                        {
                            new Dictionary<string, object> { { "Monto", 5000 } }
                        }
                    }
                }
            };

            ConciliationsRepository repository = CreateQueryRepository(null, null, projection);
            List<FuenteDetalleDto> result = await repository.GetConciliationFuentesDetailAsync("CONC-99", CancellationToken.None);

            Assert.IsNotNull(result);
            Assert.HasCount(1, result);
            Assert.AreEqual("ProcesoFacturacion Conn1", result[0].Nombre);
            Assert.HasCount(1, result[0].Datos);
            Assert.AreEqual("Monto Total", result[0].Datos[0].Nombre);
        }

        private static async Task<(Mock<IMongoCollection<HistorialConciliacionesDocument>> Historial, StatusUpdateConciliation Resultado)> EjecutarCambioEstadoAsync(
            Guid conciliacionId,
            List<BsonDocument>? historialExistente)
        {
            Mock<IAsyncCursor<ConciliacionDocument>> findCursor = CreateCursor(
            [
                new ConciliacionDocument
                {
                    ConciliacionId = conciliacionId.ToString(),
                    ParametrizacionId = "param-1"
                }
            ]);
            Mock<IMongoCollection<ConciliacionDocument>> conciliaciones = new();
            conciliaciones.Setup(c => c.FindAsync(
                It.IsAny<FilterDefinition<ConciliacionDocument>>(),
                It.IsAny<FindOptions<ConciliacionDocument, ConciliacionDocument>>(),
                It.IsAny<CancellationToken>())).ReturnsAsync(findCursor.Object);
            conciliaciones.Setup(c => c.UpdateManyAsync(
                It.IsAny<FilterDefinition<ConciliacionDocument>>(),
                It.IsAny<UpdateDefinition<ConciliacionDocument>>(),
                It.IsAny<UpdateOptions?>(),
                It.IsAny<CancellationToken>())).ReturnsAsync(new UpdateResult.Acknowledged(1, 1, null));

            Mock<IMongoCollection<EventoDocument>> eventos = new();
            eventos.Setup(c => c.InsertManyAsync(
                It.IsAny<IEnumerable<EventoDocument>>(),
                It.IsAny<InsertManyOptions>(),
                It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

            Mock<IMongoCollection<BsonDocument>> lectura = new();
            if (historialExistente == null)
            {
                lectura.Setup(c => c.FindAsync(
                    It.IsAny<FilterDefinition<BsonDocument>>(),
                    It.IsAny<FindOptions<BsonDocument, BsonDocument>>(),
                    It.IsAny<CancellationToken>())).ReturnsAsync((IAsyncCursor<BsonDocument>)null!);
            }
            else
            {
                lectura.Setup(c => c.FindAsync(
                    It.IsAny<FilterDefinition<BsonDocument>>(),
                    It.IsAny<FindOptions<BsonDocument, BsonDocument>>(),
                    It.IsAny<CancellationToken>())).ReturnsAsync(CreateCursor(historialExistente).Object);
            }

            Mock<IMongoCollection<HistorialConciliacionesDocument>> historial = new();
            historial.Setup(c => c.InsertManyAsync(
                It.IsAny<IEnumerable<HistorialConciliacionesDocument>>(),
                It.IsAny<InsertManyOptions>(),
                It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

            Mock<DocumentDBContext> context = CreateContext();
            context.Setup(c => c.GetCollection<ConciliacionDocument>(It.IsAny<string>())).Returns(conciliaciones.Object);
            context.Setup(c => c.GetCollection<EventoDocument>(It.IsAny<string>())).Returns(eventos.Object);
            context.Setup(c => c.GetCollection<HistorialConciliacionesDocument>(It.IsAny<string>())).Returns(historial.Object);
            context.Setup(c => c.GetCollection<BsonDocument>(It.IsAny<string>())).Returns(lectura.Object);

            StatusUpdateConciliation resultado = await CreateFacade(context.Object).UpdateEstadoConciliacionAsync(
                new List<Guid> { conciliacionId },
                CancellationToken.None);
            return (historial, resultado);
        }

        private static ConciliationsRepository CreateQueryRepository(
            ConciliationUiAggregationProjection? uiProjection,
            ConciliationRowsAggregationProjection? rowsProjection,
            ConciliationFuentesDetailProjection? fuentesProjection)
        {
            Mock<DocumentDBContext> contextMock = CreateContext();

            if (uiProjection != null || rowsProjection != null)
            {
                Mock<IMongoCollection<ParametrizacionDocument>> paramCollection = new Mock<IMongoCollection<ParametrizacionDocument>>();
                if (uiProjection != null)
                {
                    Mock<IAsyncCursor<ConciliationUiAggregationProjection>> cursor = CreateCursor(new List<ConciliationUiAggregationProjection> { uiProjection });
                    paramCollection.Setup(c => c.AggregateAsync(
                        It.IsAny<PipelineDefinition<ParametrizacionDocument, ConciliationUiAggregationProjection>>(),
                        It.IsAny<AggregateOptions>(),
                        It.IsAny<CancellationToken>()
                    )).ReturnsAsync(cursor.Object);
                }

                if (rowsProjection != null)
                {
                    Mock<IAsyncCursor<ConciliationRowsAggregationProjection>> cursor = CreateCursor(new List<ConciliationRowsAggregationProjection> { rowsProjection });
                    paramCollection.Setup(c => c.AggregateAsync(
                        It.IsAny<PipelineDefinition<ParametrizacionDocument, ConciliationRowsAggregationProjection>>(),
                        It.IsAny<AggregateOptions>(),
                        It.IsAny<CancellationToken>()
                    )).ReturnsAsync(cursor.Object);
                }

                contextMock.Setup(c => c.GetCollection<ParametrizacionDocument>(It.IsAny<string>())).Returns(paramCollection.Object);
            }

            if (fuentesProjection != null)
            {
                Mock<IAsyncCursor<ConciliationFuentesDetailProjection>> cursor = CreateCursor(new List<ConciliationFuentesDetailProjection> { fuentesProjection });
                Mock<IMongoCollection<ConciliacionDocument>> concCollection = new Mock<IMongoCollection<ConciliacionDocument>>();
                concCollection.Setup(c => c.AggregateAsync(
                    It.IsAny<PipelineDefinition<ConciliacionDocument, ConciliationFuentesDetailProjection>>(),
                    It.IsAny<AggregateOptions>(),
                    It.IsAny<CancellationToken>()
                )).ReturnsAsync(cursor.Object);
                contextMock.Setup(c => c.GetCollection<ConciliacionDocument>(It.IsAny<string>())).Returns(concCollection.Object);
            }

            return CreateFacade(contextMock.Object);
        }

        private static ConciliationsRepository CreateFacade(DocumentDBContext documentDBContext)
        {
            ITransformacionMontoCalculator montoCalculator = new TransformacionMontoCalculator();
            IUiConfigColumnExtractor columnExtractor = new UiConfigColumnExtractor();
            IConciliationQueryPort queryPort = new ConciliationQueryRepository(
                documentDBContext,
                new ConciliationUiComposer(),
                columnExtractor,
                new ConciliationRowsComposer(montoCalculator, columnExtractor),
                new ConciliationFuentesDetailComposer());

            return new ConciliationsRepository(
                queryPort,
                new ConciliationStateRepository(documentDBContext),
                new ParametrizacionWriteRepository(
                    documentDBContext,
                    HierarchyGetOrCreateFactory.CreateHierarchyWritePort(documentDBContext)));
        }

        private static Mock<IAsyncCursor<T>> CreateCursor<T>(List<T> items)
        {
            Mock<IAsyncCursor<T>> cursor = new Mock<IAsyncCursor<T>>();
            cursor.SetupSequence(c => c.MoveNextAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(true)
                .ReturnsAsync(false);
            cursor.Setup(c => c.Current).Returns(items);
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
