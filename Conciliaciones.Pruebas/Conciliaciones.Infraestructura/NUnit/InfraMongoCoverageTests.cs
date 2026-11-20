using Conciliaciones.Dominio.DTOs.Requests.Automatizaciones;
using Conciliaciones.Dominio.DTOs.Requests.Conciliation;
using Conciliaciones.Dominio.DTOs.Response.ExternalAPIs;
using Conciliaciones.Infraestructura.Adapters;
using Conciliaciones.Infraestructura.Persistence.DocumentDB;
using Conciliaciones.Infraestructura.Persistence.DocumentDb.Constants;
using Conciliaciones.Infraestructura.Persistence.Documents;
using Conciliaciones.Infraestructura.Persistence.Hierarchy;
using Conciliaciones.Infraestructura.Persistence.Hierarchy.Strategies;
using Conciliaciones.Infraestructura.Persistence.Repositories;
using Conciliaciones.Infraestructura.Ports.Interfaces.Persistence;
using Conciliaciones.Infraestructura.Settings;
using FluentAssertions;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Options;
using Mongo2Go;
using MongoDB.Bson;
using MongoDB.Driver;
using Moq;
using NUnit.Framework;
using System.Net;
using CreateAutomatizacionResponse = global::Conciliaciones.Dominio.DTOs.Response.Automatizaciones.CreateAutomatizacionResponse;
using CreateParametrizacionResponse = global::Conciliaciones.Dominio.DTOs.Response.Conciliation.CreateParametrizacionResponse;
using DatabaseCatalogResult = global::Conciliaciones.Dominio.Ports.Interfaces.Persistence.DatabaseCatalogResult;
using DatabasePingResult = global::Conciliaciones.Dominio.Ports.Interfaces.Persistence.DatabasePingResult;
using HierarchyIdentityPolicy = global::Conciliaciones.Dominio.Services.HierarchyIdentityPolicy;
using IDocumentDBContext = global::Conciliaciones.Infraestructura.Ports.Interfaces.DBContext.IDocumentDBContext;
using IHierarchyWritePort = global::Conciliaciones.Dominio.Ports.Interfaces.Hierarchy.IHierarchyWritePort;
using IParameterStoreService = global::Conciliaciones.Infraestructura.Ports.Interfaces.AWS.IParameterStoreService;

namespace Conciliaciones.Pruebas.Conciliaciones.Infraestructura.NUnit
{
    [TestFixture]
    public class InfraMongoCoverageTests
    {
        private static MongoDbRunner? _runner;
        private static string _connectionString = string.Empty;
        private static IDocumentDBContext _context = null!;

        [OneTimeSetUp]
        public void OneTimeSetUp()
        {
            _runner = MongoDbRunner.Start();
            _connectionString = _runner.ConnectionString;
            DocumentDbSettings settings = new()
            {
                ConnectionString = _connectionString,
                DatabaseName = "ConciliacionesCoverage",
                UseSsl = false
            };
            _context = new DocumentDBContext(Options.Create(settings));
        }

        [OneTimeTearDown]
        public void OneTimeTearDown()
        {
            _runner?.Dispose();
        }

        [Test]
        public async Task IndexInitializer_CreatesIndexes_OnAllCollections()
        {
            DocumentDbIndexInitializer initializer = new(_context);
            await initializer.EnsureIndexesAsync(CancellationToken.None);

            IMongoCollection<BsonDocument> proyectos = _context.GetCollection<BsonDocument>("proyectos");
            List<BsonDocument> indexes = await (await proyectos.Indexes.ListAsync()).ToListAsync();
            indexes.Count.Should().BeGreaterThan(1);
        }

        [Test]
        public async Task HierarchyStrategies_AndGetOrCreate_RoundTrip()
        {
            ProyectoHierarchyStrategy proyectoStrategy = new(_context);
            ProcesoHierarchyStrategy procesoStrategy = new(_context);
            SubProcesoHierarchyStrategy subStrategy = new(_context);

            GetOrCreateRepository repo = new(
                HierarchyIdentityPolicy.Instance,
                new IHierarchyEntityStrategy[] { proyectoStrategy, procesoStrategy, subStrategy });

            HierarchyWriteRepository write = new(repo);

            string proyectoId = await write.ResolveOrCreateProyectoAsync("Portal Coverage", CancellationToken.None);
            string sameProyecto = await write.ResolveOrCreateProyectoAsync("Portal Coverage", CancellationToken.None);
            sameProyecto.Should().Be(proyectoId);

            string byId = await write.ResolveOrCreateProyectoAsync(proyectoId, CancellationToken.None);
            byId.Should().Be(proyectoId);

            string procesoId = await write.ResolveOrCreateProcesoAsync("Proceso Coverage", proyectoId, CancellationToken.None);
            string subId = await write.ResolveOrCreateSubProcesoAsync("Sub Coverage", procesoId, CancellationToken.None);

            (await proyectoStrategy.ExistsByIdAsync(proyectoId, CancellationToken.None)).Should().BeTrue();
            (await proyectoStrategy.ExistsByIdAsync(" ", CancellationToken.None)).Should().BeFalse();
            (await procesoStrategy.ExistsByIdAsync(procesoId, CancellationToken.None)).Should().BeTrue();
            (await subStrategy.ExistsByIdAsync(subId, CancellationToken.None)).Should().BeTrue();
            (await procesoStrategy.FindExistingIdAsync("Proceso Coverage", proyectoId, CancellationToken.None)).Should().Be(procesoId);
            (await subStrategy.FindExistingIdAsync("Sub Coverage", procesoId, CancellationToken.None)).Should().Be(subId);
            (await subStrategy.FindExistingIdAsync("missing", procesoId, CancellationToken.None)).Should().BeNull();

            Func<Task> missingId = () => write.ResolveOrCreateProyectoAsync(Guid.NewGuid().ToString(), CancellationToken.None);
            await missingId.Should().ThrowAsync<KeyNotFoundException>();

            subId.Should().NotBeNullOrWhiteSpace();
        }

        [Test]
        public void GetOrCreateRepository_MissingStrategy_Throws()
        {
            Action act = () => _ = new GetOrCreateRepository(
                HierarchyIdentityPolicy.Instance,
                new IHierarchyEntityStrategy[] { new ProyectoHierarchyStrategy(_context) });
            act.Should().Throw<InvalidOperationException>();
        }

        [Test]
        public async Task ParametrizacionWriteRepository_CreatesAndCompensatesOnUiFailure()
        {
            Mock<IHierarchyWritePort> hierarchy = new();
            hierarchy.Setup(h => h.ResolveOrCreateProyectoAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(Guid.NewGuid().ToString());
            hierarchy.Setup(h => h.ResolveOrCreateProcesoAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(Guid.NewGuid().ToString());
            hierarchy.Setup(h => h.ResolveOrCreateSubProcesoAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(Guid.NewGuid().ToString());

            ParametrizacionWriteRepository okRepo = new(_context, hierarchy.Object);
            CreateParametrizacionRequest request = BuildRequest();
            CreateParametrizacionResponse created = await okRepo.CreateParametrizacionAsync(request, CancellationToken.None);
            created.ParametrizacionId.Should().NotBeNullOrWhiteSpace();

            Mock<IDocumentDBContext> failingCtx = new();
            Mock<IMongoCollection<ParametrizacionDocument>> paramCol = new();
            Mock<IMongoCollection<ParametrizacionUiDocument>> uiCol = new();
            paramCol.Setup(c => c.InsertOneAsync(It.IsAny<ParametrizacionDocument>(), It.IsAny<InsertOneOptions>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);
            paramCol.Setup(c => c.DeleteOneAsync(It.IsAny<FilterDefinition<ParametrizacionDocument>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new DeleteResult.Acknowledged(1));
            uiCol.Setup(c => c.InsertOneAsync(It.IsAny<ParametrizacionUiDocument>(), It.IsAny<InsertOneOptions>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new InvalidOperationException("ui fail"));

            failingCtx.Setup(c => c.GetCollection<ParametrizacionDocument>(It.IsAny<string>())).Returns(paramCol.Object);
            failingCtx.Setup(c => c.GetCollection<ParametrizacionUiDocument>(It.IsAny<string>())).Returns(uiCol.Object);

            ParametrizacionWriteRepository failRepo = new(failingCtx.Object, hierarchy.Object);
            Func<Task> act = () => failRepo.CreateParametrizacionAsync(BuildRequest(), CancellationToken.None);
            await act.Should().ThrowAsync<InvalidOperationException>();
            paramCol.Verify(c => c.DeleteOneAsync(It.IsAny<FilterDefinition<ParametrizacionDocument>>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Test]
        public async Task ProbeAndHealthCheck_HappyPaths_AgainstEmbeddedMongo()
        {
            DocumentDbProbeAdapter probe = new(_context);
            DatabasePingResult ping = await probe.PingAsync(CancellationToken.None);
            ping.Ok.Should().BeTrue();
            ping.DatabaseName.Should().Be("ConciliacionesCoverage");

            DatabaseCatalogResult catalog = await probe.ListDatabasesAsync(CancellationToken.None);
            catalog.Ok.Should().BeTrue();
            catalog.Databases.Should().Contain("ConciliacionesCoverage");

            DocumentDbReadyHealthCheck health = new(_context);
            HealthCheckResult result = await health.CheckHealthAsync(new HealthCheckContext());
            result.Status.Should().Be(HealthStatus.Healthy);
        }

        [Test]
        public async Task DocumentDBContext_InitializeAsync_FromParameterStore()
        {
            Mock<IParameterStoreService> store = new();
            store.Setup(s => s.GetValueParameterStore("ConexionMotorConciliaciones", It.IsAny<CancellationToken>()))
                .ReturnsAsync(_connectionString);

            string? prevDb = Environment.GetEnvironmentVariable("DATABASE");
            string? prevSsl = Environment.GetEnvironmentVariable("USESSL");
            try
            {
                Environment.SetEnvironmentVariable("DATABASE", "FromParamStore");
                Environment.SetEnvironmentVariable("USESSL", "false");
                DocumentDBContext ctx = new(store.Object);
                await ctx.InitializeAsync(CancellationToken.None);
                ctx.Database.DatabaseNamespace.DatabaseName.Should().Be("FromParamStore");
                await ctx.InitializeAsync(CancellationToken.None); // early-return when already init
            } finally
            {
                Environment.SetEnvironmentVariable("DATABASE", prevDb);
                Environment.SetEnvironmentVariable("USESSL", prevSsl);
            }
        }

        [Test]
        public void ExternalApisParametrizacion_CtorNulo_Lanza()
        {
            Action act = () => _ = new ExternalApisParametrizacion(null!);
            act.Should().Throw<ArgumentNullException>();
        }

        [Test]
        public async Task ExternalApisParametrizacion_PorJerarquia_RetornaReglasYParametros()
        {
            string proyectoId = Guid.NewGuid().ToString();
            string procesoId = Guid.NewGuid().ToString();
            string subProcesoId = Guid.NewGuid().ToString();
            string parametrizacionId = Guid.NewGuid().ToString();

            await _context.GetCollection<BsonDocument>(DocumentDBCollectionNames.MongoCollectionNames.Procesos)
                .InsertOneAsync(new BsonDocument
                {
                    { "idProceso", procesoId },
                    { "idProyecto", proyectoId },
                    { "nombre", "proceso-ext" }
                });
            await _context.GetCollection<BsonDocument>(DocumentDBCollectionNames.MongoCollectionNames.SubProcesos)
                .InsertOneAsync(new BsonDocument
                {
                    { "idSubProceso", subProcesoId },
                    { "idProceso", procesoId },
                    { "nombre", "sub-ext" }
                });
            await _context.GetCollection<ParametrizacionDocument>(DocumentDBCollectionNames.MongoCollectionNames.Parametrizacion)
                .InsertOneAsync(new ParametrizacionDocument
                {
                    ParametrizacionId = parametrizacionId,
                    SubProcesoId = subProcesoId,
                    Parametros = new List<ParametroDocument> { new() { Nombre = "p1", Tipo = "string" } },
                    Reglas = new List<ReglaItemDocument>
                    {
                        new()
                        {
                            IdRegla = "regla-1",
                            NombreRegla = "Regla Ext",
                            EventoExito = "ok",
                            MensajeError = "err",
                            Expresion = "true"
                        }
                    }
                });

            ExternalApisParametrizacion repo = new(_context);

            IReadOnlyList<RulesParamsResponseDto> bySub =
                await repo.GetParameterizationsByClientAsync(null, null, subProcesoId, CancellationToken.None);
            bySub.Should().ContainSingle(x => x.idParametrizacion == parametrizacionId);
            bySub[0].reglas.Should().ContainSingle(r => r.IdRegla == "regla-1");
            bySub[0].parametros.Should().NotBeNull();

            IReadOnlyList<RulesParamsResponseDto> byProceso =
                await repo.GetParameterizationsByClientAsync(null, procesoId, null, CancellationToken.None);
            byProceso.Should().Contain(x => x.idParametrizacion == parametrizacionId);

            IReadOnlyList<RulesParamsResponseDto> byProyecto =
                await repo.GetParameterizationsByClientAsync(proyectoId, null, null, CancellationToken.None);
            byProyecto.Should().Contain(x => x.idParametrizacion == parametrizacionId);

            IReadOnlyList<RulesParamsResponseDto> withHierarchyChecks =
                await repo.GetParameterizationsByClientAsync(proyectoId, procesoId, subProcesoId, CancellationToken.None);
            withHierarchyChecks.Should().ContainSingle(x => x.idParametrizacion == parametrizacionId);

            string emptyParamId = Guid.NewGuid().ToString();
            string emptySubId = Guid.NewGuid().ToString();
            await _context.GetCollection<ParametrizacionDocument>(DocumentDBCollectionNames.MongoCollectionNames.Parametrizacion)
                .InsertOneAsync(new ParametrizacionDocument
                {
                    ParametrizacionId = emptyParamId,
                    SubProcesoId = emptySubId
                });
            IReadOnlyList<RulesParamsResponseDto> emptyRules =
                await repo.GetParameterizationsByClientAsync(null, null, emptySubId, CancellationToken.None);
            emptyRules.Should().ContainSingle(x => x.idParametrizacion == emptyParamId);
            emptyRules[0].reglas.Should().BeEmpty();
            emptyRules[0].parametros.Should().BeNull();
        }

        [Test]
        public void AutomatizacionWriteRepository_CtorNulo_Lanza()
        {
            Action act = () => _ = new AutomatizacionWriteRepository(null!);
            act.Should().Throw<ArgumentNullException>();
        }

        [Test]
        public async Task AutomatizacionWriteRepository_CreateAsync_EscenariosPrincipales()
        {
            AutomatizacionWriteRepository repo = new(_context);
            Func<Task> nullRequest = () => repo.CreateAsync(null!, CancellationToken.None);
            await nullRequest.Should().ThrowAsync<ArgumentNullException>();

            (HttpStatusCode badStatus, CreateAutomatizacionResponse? badData, string badMsg) =
                await repo.CreateAsync(new CreateAutomatizacionRequest { ParametrizacionId = "  " }, CancellationToken.None);
            badStatus.Should().Be(HttpStatusCode.BadRequest);
            badData.Should().BeNull();
            badMsg.Should().Contain("parametrizacionId");

            (HttpStatusCode missingStatus, CreateAutomatizacionResponse? missingData, _) =
                await repo.CreateAsync(new CreateAutomatizacionRequest { ParametrizacionId = Guid.NewGuid().ToString() }, CancellationToken.None);
            missingStatus.Should().Be(HttpStatusCode.NotFound);
            missingData.Should().BeNull();

            string parametrizacionId = Guid.NewGuid().ToString();
            await _context.GetCollection<ParametrizacionDocument>(DocumentDBCollectionNames.MongoCollectionNames.Parametrizacion)
                .InsertOneAsync(new ParametrizacionDocument
                {
                    ParametrizacionId = parametrizacionId,
                    SubProcesoId = Guid.NewGuid().ToString()
                });

            CreateAutomatizacionRequest richRequest = new()
            {
                ParametrizacionId = parametrizacionId,
                Reintento = 1,
                Intentos = 3,
                EsperaReintento = 5,
                FechaFinal = "2026-12-31",
                PreFuentes =
                [
                    new AutomatizacionFuenteDto
                    {
                        FuenteId = "f1",
                        ConexionId = "c1",
                        Tabla = "t1",
                        CamposSeleccion =
                        [
                            new AutomatizacionCampoSeleccionDto { Nombre = "col", TipoDato = "string", Alias = "a" }
                        ],
                        Filtros =
                        [
                            new AutomatizacionFiltroDto
                            {
                                Operador = "AND",
                                Condiciones =
                                [
                                    new AutomatizacionCondicionDto { Campo = "x", Operador = "=", Valor = "1" }
                                ]
                            }
                        ]
                    }
                ],
                MarcaAgua = new MarcaAguaDto { IndiceFinal = 10, FechaBusqueda = "2026-01-01" },
                Intervalo = new IntervaloDto { Valor = 15, Unidad = "minutos" }
            };

            (HttpStatusCode okStatus, CreateAutomatizacionResponse? okData, string okMsg) =
                await repo.CreateAsync(richRequest, CancellationToken.None);
            okStatus.Should().Be(HttpStatusCode.OK);
            okData.Should().NotBeNull();
            okData!.ParametrizacionId.Should().Be(parametrizacionId);
            okData.AutomatizacionId.Should().NotBeNullOrWhiteSpace();
            okMsg.Should().Contain("creada");

            AutomatizacionDocument saved = await _context
                .GetCollection<AutomatizacionDocument>(DocumentDBCollectionNames.MongoCollectionNames.Automatizaciones)
                .Find(x => x.ParametrizacionId == parametrizacionId)
                .FirstAsync();
            saved.Estado.Should().Be("activo");
            saved.PreFuentes.Should().ContainSingle();
            saved.MarcaAgua.Should().NotBeNull();
            saved.Intervalo.Should().NotBeNull();

            (HttpStatusCode conflictStatus, CreateAutomatizacionResponse? conflictData, string conflictMsg) =
                await repo.CreateAsync(new CreateAutomatizacionRequest { ParametrizacionId = parametrizacionId }, CancellationToken.None);
            conflictStatus.Should().Be(HttpStatusCode.Conflict);
            conflictData.Should().BeNull();
            conflictMsg.Should().Contain("Ya existe");
        }

        private static CreateParametrizacionRequest BuildRequest() =>
            new()
            {
                proyecto = "p",
                proceso = "pr",
                subproceso = "sp",
                parametrizacion = new ParametrizacionBodyDto
                {
                    parametros = new List<ParametroDto> { new() { nombre = "n", tipo = "t" } },
                    fuentes = new List<FuenteParametrizacionDto>
                    {
                        new()
                        {
                            fuenteId = "f",
                            conexionId = "c",
                            tabla = "t",
                            camposSeleccion = new List<campoSeleccionDto> { new() { nombre = "a", tipoDato = "s", alias = "a" } }
                        }
                    },
                    transformacion = new List<TransformacionDto>(),
                    reglas = new List<ReglaItemDto>
                    {
                        new()
                        {
                            idRegla = Guid.NewGuid().ToString(),
                            nombreRegla = "r",
                            eventoExito = "e",
                            mensajeError = "m",
                            expresion = "x"
                        }
                    }
                },
                parametrizacionUI = new parametrizacionUiBodyDto
                {
                    fuente = new List<fuenteUiColumnaInputDto>
                    {
                        new() { fuenteDato = "f.a", tipo = "text", label = "A" }
                    },
                    datos = new List<fuenteUiDatosInputDto>()
                }
            };
    }
}
