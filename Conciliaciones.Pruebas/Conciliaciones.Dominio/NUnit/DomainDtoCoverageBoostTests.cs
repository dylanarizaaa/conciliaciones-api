using Conciliaciones.Dominio.DTOs.Requests.Automatizaciones;
using Conciliaciones.Dominio.DTOs.Requests.Conciliation;
using Conciliaciones.Dominio.DTOs.Requests.Reports;
using Conciliaciones.Dominio.DTOs.Response.Automatizaciones;
using Conciliaciones.Dominio.DTOs.Response.Conciliation;
using Conciliaciones.Dominio.DTOs.Response.ExternalAPIs;
using Conciliaciones.Dominio.DTOs.Response.Reports;
using Conciliaciones.Dominio.DTOs.Response.SubProcess;
using Conciliaciones.Dominio.Entities.Client;
using Conciliaciones.Dominio.Entities.Events;
using Conciliaciones.Dominio.Entities.Parameterization;
using Conciliaciones.Dominio.Ports.Interfaces.Persistence;
using Conciliaciones.Dominio.Settings;
using FluentAssertions;
using NUnit.Framework;

namespace Conciliaciones.Pruebas.Conciliaciones.Dominio.NUnit
{
    [TestFixture]
    public class DomainDtoCoverageBoostTests
    {
        [Test]
        public void CreateParametrizacionRequest_Graph_PropertiesRoundTrip()
        {
            CreateParametrizacionRequest req = new()
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
                            tabla = "tb",
                            camposSeleccion = new List<campoSeleccionDto> { new() { nombre = "a", tipoDato = "s", alias = "al" } },
                            filtros = new List<datosFiltros>
                            {
                                new()
                                {
                                    operador = "AND",
                                    condiciones = new List<listCondiciones> { new() { campo = "c", operador = "=", valor = "v" } }
                                }
                            }
                        }
                    },
                    transformacion = new List<TransformacionDto>
                    {
                        new()
                        {
                            alias = "a",
                            tipo = "n",
                            operacion = "o",
                            operandos = new List<OperandoDto> { new() { @ref = "r" } }
                        }
                    },
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
                    fuente = new List<fuenteUiColumnaInputDto> { new() { fuenteDato = "a", tipo = "text", label = "L" } },
                    datos = new List<fuenteUiDatosInputDto>
                    {
                        new()
                        {
                            idFuente = "f",
                            datos = new List<fuenteUiColumnaInputDto> { new() { fuenteDato = "b", tipo = "number", label = "B" } }
                        }
                    }
                }
            };

            req.proyecto.Should().Be("p");
            req.parametrizacion!.fuentes![0].filtros![0].condiciones![0].valor.Should().Be("v");
            req.parametrizacion.reglas![0].nombreRegla.Should().Be("r");
            req.parametrizacionUI!.datos[0].datos[0].label.Should().Be("B");
        }

        [Test]
        public void ResponseDtos_Entities_AndSettings_CanBeConstructed()
        {
            CellConfigDto cell = new("text", null, null);
            ColumnUiDto column = new("f", "H", true, cell);
            TableConfigsDto tableConfigs = new(new List<ColumnUiDto> { column });
            BlockUiDto block = new("1", "table", "T", tableConfigs);
            ColumsDetailsDto cols = new(new List<BlockUiDto> { block });
            cols.Blocks.Should().ContainSingle();

            TableConfigDto table = new(new List<ColumnUiDto> { column }, new List<Dictionary<string, object>> { new() });
            ConciliationResponseDto response = new(10, 20m, 1, 10, 2, new List<BlockUiDto> { block });
            response.CasosTotales.Should().Be(10);
            response.MontoAfectado.Should().Be(20m);
            response.PageNumber.Should().Be(1);
            response.PageSize.Should().Be(10);
            response.TotalPages.Should().Be(2);
            response.Blocks.Should().ContainSingle();
            column.Sortable.Should().BeTrue();
            table.Columns.Should().ContainSingle();
            table.Rows.Should().ContainSingle();
            block.Type.Should().Be("table");

            ConciliacionExportColumnDto exportCol = new("H", "k", "currency", "f1", "prop", false);
            exportCol.CellType.Should().Be("currency");
            exportCol.FuenteId.Should().Be("f1");
            exportCol.PropertyName.Should().Be("prop");
            ConciliacionExportDataDto exportData = new("regla", 3, 9.5m, new List<ConciliacionExportColumnDto> { exportCol }, Array.Empty<IReadOnlyDictionary<string, object?>>());
            exportData.NombreRegla.Should().Be("regla");
            exportData.TotalCount.Should().Be(3);
            exportData.MontoAfectado.Should().Be(9.5m);

            ExcelStyles styles = new()
            {
                Titulo = null,
                Encabezado = null,
                Normal = null,
                Moneda = null,
                Fecha = null,
                Porcentaje = null,
                Entero = null,
                Decimal = null
            };
            styles.Titulo.Should().BeNull();
            styles.Encabezado.Should().BeNull();
            styles.Normal.Should().BeNull();
            styles.Moneda.Should().BeNull();
            styles.Fecha.Should().BeNull();
            styles.Porcentaje.Should().BeNull();
            styles.Entero.Should().BeNull();
            styles.Decimal.Should().BeNull();

            PagedRowsDto rows = new(1, 1m, 1, 1, 1, 10, new List<Dictionary<string, object?>>());
            rows.PageSize.Should().Be(10);

            DatoDetalleDto dato = new("n", "string", 1);
            dato.Tipo.Should().Be("string");
            dato.Valor.Should().Be(1);
            FuenteDetalleDto fuente = new("f", new List<DatoDetalleDto> { dato });
            fuente.Datos.Should().ContainSingle();

            FailedRuleDto failed = new("id-r", "r", 1, 2m);
            failed.IdRegla.Should().Be("id-r");
            failed.nombreRegla.Should().Be("r");
            SubprocesoReglasResumenDto resumen = new(1, 2m, 1, 10, 1, new List<FailedRuleDto> { failed });
            resumen.PageNumber.Should().Be(1);
            resumen.PageSize.Should().Be(10);

            CreateAutomatizacionRequest autoReq = new()
            {
                ParametrizacionId = "p-auto",
                Estado = "activo",
                FechaFinal = "2026-12-31",
                Reintento = 1,
                Intentos = 2,
                EsperaReintento = 3,
                PreFuentes =
                [
                    new AutomatizacionFuenteDto
                    {
                        FuenteId = "f",
                        ConexionId = "c",
                        Tabla = "t",
                        CamposSeleccion = [new AutomatizacionCampoSeleccionDto { Nombre = "n", TipoDato = "s", Alias = "a" }],
                        Filtros =
                        [
                            new AutomatizacionFiltroDto
                            {
                                Operador = "AND",
                                Condiciones = [new AutomatizacionCondicionDto { Campo = "x", Operador = "=", Valor = "1" }]
                            }
                        ]
                    }
                ],
                MarcaAgua = new MarcaAguaDto { IndiceFinal = 1, FechaBusqueda = "2026-01-01" },
                Intervalo = new IntervaloDto { Valor = 10, Unidad = "min" }
            };
            autoReq.PreFuentes.Should().ContainSingle();
            autoReq.MarcaAgua!.IndiceFinal.Should().Be(1);
            autoReq.Intervalo!.Unidad.Should().Be("min");

            CreateAutomatizacionResponse autoResp = new("a1", "p-auto", "ok");
            autoResp.AutomatizacionId.Should().Be("a1");

            RulesParamsResponseDto rules = new(
                "param-1",
                [new Dictionary<string, string> { ["nombre"] = "n", ["tipo"] = "t" }],
                [new RuleParamDto("r1", "ReglaUno")]);
            rules.idParametrizacion.Should().Be("param-1");
            rules.reglas[0].IdRegla.Should().Be("r1");

            DatabasePingResult ping = new(true, "ok", "ping-raw", "db", Array.Empty<string>(), null, null);
            ping.PingResult.Should().Be("ping-raw");
            DatabaseCatalogResult catalog = new(false, "err-status", Array.Empty<string>(), "db", "err", null);
            catalog.Status.Should().Be("err-status");
            catalog.ErrorMessage.Should().Be("err");

            AwsSettings aws = new()
            {
                Region = "us-east-1",
                AccessKey = "a",
                SecretKey = "s",
                SessionToken = "t"
            };
            aws.SessionToken.Should().Be("t");

            EventsEntity ev = new()
            {
                Id = "1",
                EventoId = "e",
                ParametrizacionId = "p",
                FechaCreacion = DateTime.UtcNow,
                Parametros = new Dictionary<string, object> { ["k"] = 1 },
                ConciliacionId = "c-1",
                Reintento = true
            };
            ev.Reintento.Should().BeTrue();
            ev.ConciliacionId.Should().Be("c-1");

            Proyecto proyecto = Proyecto.Create("Portal");
            Proyecto rehydrated = Proyecto.Rehydrate(proyecto.IdProyecto, proyecto.Nombre, DateTime.UtcNow);
            rehydrated.Nombre.Should().Be("Portal");
#pragma warning disable CS0618
            Proyect legacyProyecto = new() { Id = "1", IdProyecto = "1", Nombre = "n" };
            legacyProyecto.Id.Should().Be("1");
            legacyProyecto.IdProyecto.Should().Be("1");
            legacyProyecto.Nombre.Should().Be("n");

            Proceso proceso = Proceso.Create("Proc", proyecto.IdProyecto);
            Process legacyProceso = new() { Id = "1", IdProyecto = "2", IdProceso = "3", Nombre = "n" };
            legacyProceso.Id.Should().Be("1");
            legacyProceso.IdProyecto.Should().Be("2");
            legacyProceso.IdProceso.Should().Be("3");
            legacyProceso.Nombre.Should().Be("n");

            SubProceso sub = SubProceso.Create("Sub", proceso.IdProceso);
            SubProcess legacySub = new() { Id = "1", IdProceso = "2", IdSubproceso = "3", Nombre = "n" };
            legacySub.Id.Should().Be("1");
            legacySub.IdProceso.Should().Be("2");
            legacySub.IdSubproceso.Should().Be("3");
            legacySub.Nombre.Should().Be("n");
#pragma warning restore CS0618

            UiCellConfigEntity.Text().Type.Should().Be("text");
            sub.Nombre.Should().Be("Sub");
        }
    }
}
