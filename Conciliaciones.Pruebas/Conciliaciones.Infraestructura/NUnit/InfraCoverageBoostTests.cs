using Conciliaciones.Dominio.DTOs.Requests.Conciliation;
using Conciliaciones.Dominio.DTOs.Response.Conciliation;
using Conciliaciones.Dominio.Ports.Interfaces.Persistence;
using Conciliaciones.Infraestructura.Adapters;
using Conciliaciones.Infraestructura.Persistence.DocumentDB;
using Conciliaciones.Infraestructura.Persistence.Documents;
using Conciliaciones.Infraestructura.Persistence.Documents.Factories;
using Conciliaciones.Infraestructura.Persistence.Documents.Projections;
using Conciliaciones.Infraestructura.Persistence.Hierarchy;
using Conciliaciones.Infraestructura.Persistence.Hierarchy.Strategies;
using Conciliaciones.Infraestructura.Persistence.Mappers;
using Conciliaciones.Infraestructura.Persistence.Services;
using Conciliaciones.Infraestructura.Persistence.Services.Ui;
using Conciliaciones.Infraestructura.Ports.Interfaces.DBContext;
using Conciliaciones.Infraestructura.Ports.Interfaces.Persistence;
using Conciliaciones.Infraestructura.Settings;
using FluentAssertions;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Driver;
using Moq;
using NUnit.Framework;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using HierarchyIdentityPolicy = global::Conciliaciones.Dominio.Services.HierarchyIdentityPolicy;
using IHierarchyWritePort = global::Conciliaciones.Dominio.Ports.Interfaces.Hierarchy.IHierarchyWritePort;
using ParametrizacionWriteRepository = global::Conciliaciones.Infraestructura.Persistence.Repositories.ParametrizacionWriteRepository;
using TransformacionMontoCalculator = global::Conciliaciones.Dominio.Services.TransformacionMontoCalculator;

namespace Conciliaciones.Pruebas.Conciliaciones.Infraestructura.NUnit
{
    [TestFixture]
    public class InfraCoverageBoostTests
    {
        [Test]
        public void HierarchyDocumentFactory_RoundTrip_AllEntityKinds()
        {
            ProyectoDocument proyecto = HierarchyDocumentFactory.CreateProyecto(Guid.NewGuid().ToString(), "Portal");
            ProcesoDocument proceso = HierarchyDocumentFactory.CreateProceso(Guid.NewGuid().ToString(), "Ventas", proyecto.IdProyecto);
            SubProcesoDocument sub = HierarchyDocumentFactory.CreateSubProceso(Guid.NewGuid().ToString(), "SP", proceso.IdProceso);

            HierarchyDocumentFactory.ToDomain(proyecto).Nombre.Should().Be("Portal");
            HierarchyDocumentFactory.ToDomain(proceso).Nombre.Should().Be("Ventas");
            HierarchyDocumentFactory.ToDomain(sub).Nombre.Should().Be("SP");

            proyecto.FechaCreacion.Should().NotBeNullOrWhiteSpace();
            proceso.IdProyecto.Should().Be(proyecto.IdProyecto);
            sub.IdProceso.Should().Be(proceso.IdProceso);
        }

        [Test]
        public void ParametrizacionDocumentFactory_FromRequest_FullBody_MapsGraph()
        {
            ParametrizacionBodyDto body = new()
            {
                parametros = new List<ParametroDto> { new() { nombre = "p", tipo = "string" } },
                fuentes = new List<FuenteParametrizacionDto>
                {
                    new()
                    {
                        fuenteId = "f1",
                        conexionId = "c1",
                        tabla = "t",
                        camposSeleccion = new List<campoSeleccionDto>
                        {
                            new() { nombre = "n", tipoDato = "number", alias = "a" }
                        },
                        filtros = new List<datosFiltros>
                        {
                            new()
                            {
                                operador = "AND",
                                condiciones = new List<listCondiciones>
                                {
                                    new() { campo = "x", operador = "EQUALS", valor = "1" }
                                }
                            }
                        }
                    }
                },
                transformacion = new List<TransformacionDto>
                {
                    new()
                    {
                        alias = "diff",
                        tipo = "number",
                        operacion = "resta",
                        operandos = new List<OperandoDto> { new() { @ref = "a.b" } }
                    }
                },
                reglas = new List<ReglaItemDto>
                {
                    new()
                    {
                        idRegla = "d75f5195-9109-4aea-a617-9c114dbc0443",
                        nombreRegla = "r1",
                        eventoExito = "ok",
                        mensajeError = "err",
                        expresion = "true"
                    }
                }
            };

            ParametrizacionDocument doc = ParametrizacionDocumentFactory.FromRequest(
                body,
                Guid.NewGuid().ToString(),
                Guid.NewGuid().ToString());

            doc.Parametros.Should().ContainSingle();
            doc.Fuentes.Should().ContainSingle();
            doc.Fuentes[0].CamposSeleccion.Should().ContainSingle();
            doc.Fuentes[0].Filtros.Should().ContainSingle();
            doc.Transformacion.Should().ContainSingle();
            doc.Reglas.Should().ContainSingle();
            doc.Reglas![0].IdRegla.Should().Be("d75f5195-9109-4aea-a617-9c114dbc0443");
            doc.Reglas[0].NombreRegla.Should().Be("r1");
            doc.FechaCreacion.Should();
        }

        [Test]
        public void ParametrizacionDocumentFactory_NullBody_CreatesMinimal()
        {
            ParametrizacionDocument doc = ParametrizacionDocumentFactory.FromRequest(
                null,
                Guid.NewGuid().ToString(),
                Guid.NewGuid().ToString());
            doc.Fuentes.Should().BeEmpty();
            doc.Transformacion.Should().BeEmpty();
            doc.Reglas.Should().BeEmpty();
        }

        [Test]
        public void ParametrizacionUiDocumentFactory_FromRequest_AddsEstadoAndAccionColumns()
        {
            parametrizacionUiBodyDto ui = new()
            {
                fuente = new List<fuenteUiColumnaInputDto>
                {
                    new() { fuenteDato = "campo.a", tipo = "currency", label = "Monto" },
                    new() { fuenteDato = "campo.b", tipo = "date", label = "Fecha" }
                },
                datos = new List<fuenteUiDatosInputDto>
                {
                    new()
                    {
                        idFuente = "f1",
                        datos = new List<fuenteUiColumnaInputDto>
                        {
                            new() { fuenteDato = "x.y", tipo = "text", label = "X" }
                        }
                    }
                }
            };

            ParametrizacionUiDocument doc = ParametrizacionUiDocumentFactory.FromRequest(Guid.NewGuid().ToString(), ui);
            doc.Fuente.Should().Contain(c => c.Field == "estado");
            doc.Fuente.Should().Contain(c => c.Field == "accion");
            doc.Fuente.Should().Contain(c => c.Field == "campo.a" && c.CellConfig != null);
            doc.Datos.Should().ContainSingle();
            doc.Datos[0].Columns.Should().ContainSingle();
        }

        [Test]
        public void ParametrizacionUiDocumentFactory_NullUi_StillCreatesDefaults()
        {
            ParametrizacionUiDocument doc = ParametrizacionUiDocumentFactory.FromRequest(Guid.NewGuid().ToString(), null);
            doc.Fuente.Should().HaveCount(2); // estado + accion
            doc.Datos.Should().BeEmpty();
        }

        [Test]
        public void UiCellConfigFactory_AppliesCurrencyAndDateStrategies()
        {
            UiCellConfigFactory factory = new();
            UiCellConfigDocument currency = factory.Create("currency");
            currency.Type.Should().Be("currency");
            currency.Config.Should().ContainKey("currencyCode");

            UiCellConfigDocument date = factory.Create("date");
            date.Type.Should().Be("date");
            date.Format.Should().NotBeNullOrWhiteSpace();

            factory.Create(null).Type.Should().Be("text");
            factory.Create("  ").Type.Should().Be("text");
        }

        [Test]
        public void DocumentDbSettingsFactory_ReadsEnv_AndResolvesCert()
        {
            string? prevDb = Environment.GetEnvironmentVariable("DATABASE");
            string? prevSsl = Environment.GetEnvironmentVariable("USESSL");
            string? prevLegacy = Environment.GetEnvironmentVariable("USSSSL");
            string? prevCert = Environment.GetEnvironmentVariable("CACERTIFICATEPATH");
            try
            {
                Environment.SetEnvironmentVariable("DATABASE", "ConciliacionesTest");
                Environment.SetEnvironmentVariable("USESSL", "false");
                Environment.SetEnvironmentVariable("USSSSL", null);
                Environment.SetEnvironmentVariable("CACERTIFICATEPATH", "Certificados/global-bundle.pem");

                DocumentDbSettings settings = DocumentDbSettingsFactory.FromEnvironment("mongodb://localhost:27017");
                settings.DatabaseName.Should().Be("ConciliacionesTest");
                settings.UseSsl.Should().BeFalse();
                settings.CaCertificatePath.Should().NotBeNullOrWhiteSpace();
                settings.ConnectionString.Should().Contain("localhost");
            } finally
            {
                Environment.SetEnvironmentVariable("DATABASE", prevDb);
                Environment.SetEnvironmentVariable("USESSL", prevSsl);
                Environment.SetEnvironmentVariable("USSSSL", prevLegacy);
                Environment.SetEnvironmentVariable("CACERTIFICATEPATH", prevCert);
            }
        }

        [Test]
        public void DocumentDbSettingsFactory_EmptyConnection_Throws()
        {
            Action act = () => DocumentDbSettingsFactory.FromEnvironment(" ");
            act.Should().Throw<ArgumentException>();
        }

        [Test]
        public void DocumentDbSettingsFactory_ResolveCa_WhenMissing_UsesConfiguredFallback()
        {
            Func<string, bool> previous = DocumentDbSettingsFactory.FileExists;
            try
            {
                DocumentDbSettingsFactory.FileExists = _ => false;

                string absoluteMissing = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"), "missing.pem");
                DocumentDbSettingsFactory.ResolveCaCertificatePath(absoluteMissing).Should().Be(absoluteMissing);

                string relativeMissing = $"no-cert-{Guid.NewGuid():N}.pem";
                string resolvedRelative = DocumentDbSettingsFactory.ResolveCaCertificatePath(relativeMissing);
                resolvedRelative.Should().Contain(relativeMissing);
            } finally
            {
                DocumentDbSettingsFactory.FileExists = previous;
            }
        }

        [Test]
        public async Task DocumentDbStartupHostedService_IndexesOk_CompletesTryBlock()
        {
            Mock<IDocumentDBContext> ctx = new();
            ctx.Setup(c => c.InitializeAsync(It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
            Mock<IDocumentDbIndexInitializer> indexes = new();
            indexes.Setup(i => i.EnsureIndexesAsync(It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
            Mock<ILogger<DocumentDbStartupHostedService>> logger = new();

            DocumentDbStartupHostedService hosted = new(ctx.Object, indexes.Object, logger.Object);
            await hosted.StartAsync(CancellationToken.None);
            indexes.Verify(i => i.EnsureIndexesAsync(It.IsAny<CancellationToken>()), Times.Once);
        }

        [Test]
        public void DocumentDbTlsConfigurator_WhenSslDisabled_DoesNothing()
        {
            MongoClientSettings mongo = MongoClientSettings.FromConnectionString("mongodb://localhost:27017");
            DocumentDbTlsConfigurator.Apply(mongo, new DocumentDbSettings { UseSsl = false });
            mongo.UseTls.Should().BeFalse();
        }

        [Test]
        public void DocumentDbTlsConfigurator_WhenSslEnabledAndCertMissing_EnablesTlsWithoutCallback()
        {
            MongoClientSettings mongo = MongoClientSettings.FromConnectionString("mongodb://localhost:27017");
            DocumentDbTlsConfigurator.Apply(mongo, new DocumentDbSettings
            {
                UseSsl = true,
                CaCertificatePath = Path.Combine(Path.GetTempPath(), Guid.NewGuid() + ".pem")
            });
            mongo.UseTls.Should().BeTrue();
        }

        [Test]
        public async Task DocumentDbReadyHealthCheck_WhenPingFails_ReturnsUnhealthy()
        {
            Mock<IDocumentDBContext> ctx = new();
            ctx.SetupGet(c => c.Database).Throws(new InvalidOperationException("no init"));
            DocumentDbReadyHealthCheck check = new(ctx.Object);

            HealthCheckResult result = await check.CheckHealthAsync(new HealthCheckContext());
            result.Status.Should().Be(HealthStatus.Unhealthy);
        }

        [Test]
        public async Task DocumentDbStartupHostedService_IndexFailure_IsLoggedAndDoesNotThrow()
        {
            Mock<IDocumentDBContext> ctx = new();
            ctx.Setup(c => c.InitializeAsync(It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
            Mock<IDocumentDbIndexInitializer> indexes = new();
            indexes.Setup(i => i.EnsureIndexesAsync(It.IsAny<CancellationToken>()))
                .ThrowsAsync(new Exception("no privilege"));
            Mock<ILogger<DocumentDbStartupHostedService>> logger = new();

            DocumentDbStartupHostedService hosted = new(ctx.Object, indexes.Object, logger.Object);
            await hosted.StartAsync(CancellationToken.None);
            await hosted.StopAsync(CancellationToken.None);

            ctx.Verify(c => c.InitializeAsync(It.IsAny<CancellationToken>()), Times.Once);
        }

        [Test]
        public async Task DocumentDbProbeAdapter_WhenDatabaseThrows_ReturnsFailedResults()
        {
            Mock<IDocumentDBContext> ctx = new();
            ctx.SetupGet(c => c.Database).Throws(new Exception("boom", new Exception("inner")));
            DocumentDbProbeAdapter probe = new(ctx.Object);

            DatabasePingResult ping = await probe.PingAsync(CancellationToken.None);
            ping.Ok.Should().BeFalse();
            ping.ErrorMessage.Should().NotBeNullOrWhiteSpace();

            DatabaseCatalogResult catalog = await probe.ListDatabasesAsync(CancellationToken.None);
            catalog.Ok.Should().BeFalse();
        }

        [Test]
        public void DocumentDBContext_OptionsCtor_ConfiguresDatabase()
        {
            IOptions<DocumentDbSettings> options = Options.Create(new DocumentDbSettings
            {
                ConnectionString = "mongodb://localhost:27017",
                DatabaseName = "UnitTestDb",
                UseSsl = false
            });
            DocumentDBContext ctx = new(options);
            ctx.Database.DatabaseNamespace.DatabaseName.Should().Be("UnitTestDb");
            Action badName = () => ctx.GetCollection<BsonDocument>(" ");
            badName.Should().Throw<ArgumentNullException>();
        }

        [Test]
        public void Documents_PropertyBags_CanBeAssigned()
        {
            ConciliacionDocument conc = new()
            {
                ConciliacionId = Guid.NewGuid().ToString(),
                ParametrizacionId = "p",
                Estado = "fallido",
                FechaCreacion = DateTime.UtcNow,
                ParametrosEntrada = new Dictionary<string, object> { ["a"] = 1 },
                ResultadoTransformacion = new Dictionary<string, object> { ["m"] = 1m },
                ResultadoFuentes = new List<ResultadoFuenteDocument>
                {
                    new() { FuenteId = "f", ConexionId = "c", Data = new List<Dictionary<string, object>>() }
                },
                ResultadoRegla = new ResultadoReglaDocument
                {
                    NombreRegla = "r",
                    Cumplida = false,
                    EventoExito = "e",
                    MensajeError = "m"
                }
            };
            conc.ResultadoRegla!.NombreRegla.Should().Be("r");

            ParametrizacionDocument param = new()
            {
                ParametrizacionId = "id",
                SubProcesoId = "sp",
                FechaCreacion = DateTime.UtcNow,
                Parametros = new List<ParametroDocument> { new() { Nombre = "n", Tipo = "t" } },
                Fuentes = new List<FuenteParametrizacionDocument>(),
                Transformacion = new List<TransformacionDocument>(),
                Reglas = new List<ReglaItemDocument>
                {
                    new()
                    {
                        IdRegla = "r",
                        NombreRegla = "ReglaX",
                        EventoExito = "ok",
                        MensajeError = "err",
                        Expresion = "true"
                    }
                }
            };
            param.Parametros!.Should().ContainSingle();
            param.Reglas.Should().ContainSingle();

            new ProyectoDocument { IdProyecto = "1", Nombre = "n", FechaCreacion = "x" }.Nombre.Should().Be("n");
            new ProcesoDocument { IdProceso = "1", IdProyecto = "2", Nombre = "n", FechaCreacion = "x" }.IdProyecto.Should().Be("2");
            new SubProcesoDocument { IdSubProceso = "1", IdProceso = "2", Nombre = "n", FechaCreacion = "x" }.IdProceso.Should().Be("2");
        }

        [Test]
        public void GetOrCreateRepository_MissingStrategy_Throws()
        {
            Action act = () => _ = new GetOrCreateRepository(
                HierarchyIdentityPolicy.Instance,
                Array.Empty<IHierarchyEntityStrategy>());
            act.Should().Throw<InvalidOperationException>();
        }

        [Test]
        public async Task ParametrizacionWriteRepository_NullDeps_Throw()
        {
            Action a1 = () => _ = new ParametrizacionWriteRepository(null!, Mock.Of<IHierarchyWritePort>());
            Action a2 = () => _ = new ParametrizacionWriteRepository(Mock.Of<IDocumentDBContext>(), null!);
            a1.Should().Throw<ArgumentNullException>();
            a2.Should().Throw<ArgumentNullException>();
            await Task.CompletedTask;
        }

        [Test]
        public void UiConfigColumnExtractor_LegacyBlocks_AndMappings()
        {
            UiConfigColumnExtractor extractor = new();
            extractor.ExtractColumns(null).Should().BeEmpty();
            extractor.ExtractVisibleFieldMappings(null).Should().BeEmpty();

            ParametrizacionUiDocument legacy = new()
            {
                Blocks = new List<UiLegacyBlockDocument>
                {
                    new()
                    {
                        Config = new UiLegacyBlockConfigDocument
                        {
                            Columns = new List<UiColumnDocument>
                            {
                                new() { Field = "a.b", Header = "B", Visible = true, Order = 1 }
                            }
                        }
                    }
                }
            };
            extractor.ExtractColumns(legacy).Should().ContainSingle();

            ParametrizacionUiDocument fuenteOnly = new()
            {
                Fuente = new List<UiColumnDocument>
                {
                    new() { Field = "estado", Visible = true, Order = 1 },
                    new() { Field = "", Visible = true, Order = 2 },
                    new() { Field = "f.monto", Visible = true, Order = 3 }
                }
            };
            List<(string FuenteId, string PropertyName)> mappings = extractor.ExtractVisibleFieldMappings(fuenteOnly);
            mappings.Should().Contain(m => m.PropertyName == "estado" && m.FuenteId == string.Empty);
            mappings.Should().Contain(m => m.FuenteId == "f" && m.PropertyName == "monto");
        }

        [Test]
        public void ConciliationRowsComposer_NullGroup_AndSystemFields()
        {
            ConciliationRowsComposer composer = new(
                new TransformacionMontoCalculator(),
                new UiConfigColumnExtractor());

            PagedRowsDto empty = composer.Compose(null, 1, 10);
            empty.TotalCount.Should().Be(0);

            ConciliationRowsAggregationProjection projection = new()
            {
                TotalCount = 1,
                UiConfigDoc = new ParametrizacionUiDocument
                {
                    Fuente = new List<UiColumnDocument>
                    {
                        new() { Field = "estado", Visible = true, Order = 1 },
                        new() { Field = "f.valor", Visible = true, Order = 2 }
                    }
                },
                ConciliacionesPaginadas = new List<ConciliacionDocument>
                {
                    new()
                    {
                        ConciliacionId = "c1",
                        Estado = "fallido",
                        ResultadoFuentes = new List<ResultadoFuenteDocument>
                        {
                            new()
                            {
                                FuenteId = "f",
                                Data = new List<Dictionary<string, object>>
                                {
                                    new() { ["valor"] = 10 }
                                }
                            }
                        },
                        ResultadoTransformacion = new Dictionary<string, object> { ["m"] = 5 }
                    }
                }
            };

            PagedRowsDto page = composer.Compose(projection, 1, 10);
            page.Rows.Should().ContainSingle();
            page.Rows[0].Should().ContainKey("valor");
            page.Rows[0].Should().NotContainKey("estado");
        }

        [Test]
        public void ConciliationFuentesDetailComposer_NullAndEmptyFuentes()
        {
            ConciliationFuentesDetailComposer composer = new();
            composer.Compose(null).Should().BeEmpty();
            composer.Compose(new ConciliationFuentesDetailProjection
            {
                NombreProceso = " ",
                ResultadoFuentes = null,
                UiConfigDoc = null
            }).Should().BeEmpty();
        }

        [Test]
        public void TransformacionMontoReader_NullInputs_AndNullValues()
        {
            TransformacionMontoCalculator calc = new();
            TransformacionMontoReader.SumFromConciliaciones(null, calc).Should().Be(0);
            TransformacionMontoReader.SumFromConciliaciones(
                new List<Dictionary<string, object>?> { null, new() { ["a"] = null!, ["b"] = 2 } },
                calc).Should().Be(2);
        }

        [Test]
        public void DocumentDbTlsConfigurator_WithCert_RegistersValidationCallback()
        {
            string certPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid() + ".cer");
            try
            {
                using System.Security.Cryptography.RSA rsa = System.Security.Cryptography.RSA.Create(2048);
                CertificateRequest req = new("CN=CoverageCA", rsa, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
                using X509Certificate2 cert = req.CreateSelfSigned(DateTimeOffset.UtcNow.AddDays(-1), DateTimeOffset.UtcNow.AddYears(1));
                File.WriteAllBytes(certPath, cert.Export(X509ContentType.Cert));

                MongoClientSettings mongo = MongoClientSettings.FromConnectionString("mongodb://localhost:27017");
                DocumentDbTlsConfigurator.Apply(mongo, new DocumentDbSettings
                {
                    UseSsl = true,
                    CaCertificatePath = certPath
                });
                mongo.UseTls.Should().BeTrue();
                mongo.SslSettings.Should().NotBeNull();
                mongo.SslSettings!.ServerCertificateValidationCallback.Should().NotBeNull();

                bool okNone = mongo.SslSettings.ServerCertificateValidationCallback!(
                    null, cert, null, System.Net.Security.SslPolicyErrors.None);
                okNone.Should().BeTrue();

                bool okChain = mongo.SslSettings.ServerCertificateValidationCallback!(
                    null, cert, null, System.Net.Security.SslPolicyErrors.RemoteCertificateChainErrors);
                okChain.Should().BeTrue();
            } finally
            {
                if (File.Exists(certPath))
                {
                    File.Delete(certPath);
                }
            }
        }

        [Test]
        public void DocumentDBContext_ParameterlessCtor_ThrowsUntilConfigured()
        {
            DocumentDBContext ctx = new();
            Action act = () => _ = ctx.Database;
            act.Should().Throw<InvalidOperationException>();
        }

        [Test]
        public void GetOrCreateRepository_NullStrategies_Throws()
        {
            Action act = () => _ = new GetOrCreateRepository(HierarchyIdentityPolicy.Instance, null!);
            act.Should().Throw<ArgumentNullException>();
        }

        [Test]
        public async Task HierarchyStrategies_MissingParent_Throws()
        {
            Mock<IDocumentDBContext> ctx = new();
            ProcesoHierarchyStrategy proceso = new(ctx.Object);
            SubProcesoHierarchyStrategy sub = new(ctx.Object);

            Func<Task> a1 = () => proceso.FindExistingIdAsync("n", null, CancellationToken.None);
            Func<Task> a2 = () => sub.FindExistingIdAsync("n", " ", CancellationToken.None);
            await a1.Should().ThrowAsync<ArgumentException>();
            await a2.Should().ThrowAsync<ArgumentException>();
        }

        [Test]
        public void ProjectionAndDocument_Getters_AreReadable()
        {
            ConciliationUiAggregationProjection ui = new()
            {
                ConciliacionesFiltradas = new List<ConciliacionDocument>()
            };
            ui.ConciliacionesFiltradas.Should().BeEmpty();

            ConciliationFuentesDetailProjection detail = new() { ConciliacionId = "c1" };
            detail.ConciliacionId.Should().Be("c1");

            ConciliacionDocument doc = new() { Id = MongoDB.Bson.ObjectId.GenerateNewId() };
            doc.Id.Should().NotBe(default);
        }
    }
}
