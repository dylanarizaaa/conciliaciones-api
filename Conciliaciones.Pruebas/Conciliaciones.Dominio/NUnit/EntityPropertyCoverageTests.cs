using Conciliaciones.Dominio.Entities.Client;
using Conciliaciones.Dominio.Entities.Common;
using Conciliaciones.Dominio.Entities.Conciliation;
using Conciliaciones.Dominio.Entities.Parameterization;
using FluentAssertions;
using NUnit.Framework;
using DomainValidationException = Conciliaciones.Dominio.Exceptions.ValidationException;

namespace Conciliaciones.Pruebas.Conciliaciones.Dominio.NUnit
{
    /// <summary>Cubre getters y ramas de constructores no ejercitados por factories/validators.</summary>
    [TestFixture]
    public class EntityPropertyCoverageTests
    {
        [Test]
        public void Parameterization_FullGraph_ExposesAllProperties()
        {
            // Arrange
            ParametroEntity parametro = new(" nombre ", " tipo ");
            CampoSeleccionEntity campo = new("col", "int", "alias");
            CondicionFiltroEntity condicion = new("a", "=", "1");
            FiltroEntity filtro = new("AND", new[] { condicion });
            FuenteEntity fuente = new("f1", "c1", "tabla", new[] { campo }, new[] { filtro });
            OperandoEntity operando = new("ref1");
            TransformacionEntity transformacion = new("diff", "number", "sub", new[] { operando });
            ReglaEntity regla = new("r", "ok", "err", "expr", "id-regla");

            // Act
            ParameterizationEntity entity = ParameterizationEntity.Create(
                Guid.NewGuid().ToString(),
                Guid.NewGuid().ToString(),
                new[] { parametro },
                new[] { fuente },
                new[] { transformacion },
                new[] { regla });

            // Assert — fuerza cobertura de getters
            entity.FechaCreacion.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
            parametro.Nombre.Should().Be("nombre");
            parametro.Tipo.Should().Be("tipo");
            fuente.ConexionId.Should().Be("c1");
            fuente.CamposSeleccion.Should().ContainSingle();
            campo.Nombre.Should().Be("col");
            campo.TipoDato.Should().Be("int");
            campo.Alias.Should().Be("alias");
            filtro.Operador.Should().Be("AND");
            filtro.Condiciones.Should().ContainSingle();
            condicion.Campo.Should().Be("a");
            condicion.Operador.Should().Be("=");
            condicion.Valor.Should().Be("1");
            transformacion.Alias.Should().Be("diff");
            transformacion.Tipo.Should().Be("number");
            transformacion.Operacion.Should().Be("sub");
            operando.Ref.Should().Be("ref1");
            regla.IdRegla.Should().Be("id-regla");
            regla.EventoExito.Should().Be("ok");
            regla.MensajeError.Should().Be("err");
            regla.Expresion.Should().Be("expr");
            entity.Reglas.Should().ContainSingle();
        }

        [Test]
        public void ParameterizationUI_CurrencyAndFlags_ExposeProperties()
        {
            // Arrange
            CurrencyCellConfigEntity currency = CurrencyCellConfigEntity.CopDefault();
            UiCellConfigEntity cell = UiCellConfigEntity.CurrencyCop();
            UiColumnEntity column = new("amount", "Monto", 1, sortable: false, visible: false, cellConfig: cell);
            UiDatosBlockEntity block = new("f1", new[] { column });
            ParameterizationUIEntity ui = ParameterizationUIEntity.Create(Guid.NewGuid().ToString(), new[] { column }, new[] { block });

            // Assert
            column.Sortable.Should().BeFalse();
            column.Visible.Should().BeFalse();
            cell.Currency.Should().NotBeNull();
            cell.VariantByValue.Should().BeNull();
            currency.Display.Should().Be("symbol");
            currency.DigitsInfo.Should().Be("1.0-2");
            currency.Locale.Should().Be("es-CO");
            currency.Symbol.Should().Be("$");
            ui.Datos.Should().ContainSingle();
        }

        [Test]
        public void Conciliacion_Rehydrate_ExposesIdentifiersAndFuentes()
        {
            // Arrange / Act
            ResultadoFuenteEntity fuente = new("f", "c", null);
            ConciliacionEntity entity = ConciliacionEntity.Rehydrate(
                Guid.NewGuid().ToString(),
                "param-1",
                ConciliacionEstados.con_novedad,
                new Dictionary<string, object> { ["x"] = 1 },
                new[] { fuente },
                null);

            // Assert
            entity.ConciliacionId.Should().NotBeNullOrWhiteSpace();
            entity.ParametrizacionId.Should().Be("param-1");
            entity.ResultadoTransformacion.Should().ContainKey("x");
            fuente.FuenteId.Should().Be("f");
            fuente.ConexionId.Should().Be("c");
            entity.EsFallidaOError.Should().BeTrue();
        }

        [Test]
        public void Proyecto_Create_ExposesFechaCreacion()
        {
            Proyecto proyecto = Proyecto.Create("Proyecto X");
            proyecto.FechaCreacion.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
            proyecto.Nombre.Should().Be("Proyecto X");
        }

        [Test]
        public void ConciliacionEstados_NormalizeOrThrow_AllowsKnownAndRejectsUnknown()
        {
            ConciliacionEstados.NormalizeOrThrow(null).Should().Be("pendiente");
            ConciliacionEstados.NormalizeOrThrow("  Resuelto  ").Should().Be(ConciliacionEstados.Resuelto);
            ConciliacionEstados.NormalizeOrThrow("PENDIENTE").Should().Be("pendiente");

            Action act = () => ConciliacionEstados.NormalizeOrThrow("inventado");
            act.Should().Throw<DomainValidationException>()
                .Which.Errors.Should().Contain(e => e.Contains("estado", StringComparison.OrdinalIgnoreCase));
        }

        [Test]
        public void PaginationLimits_ClampsPageSizeAndPageNumber()
        {
            PaginationLimits.NormalizePageNumber(0).Should().Be(1);
            PaginationLimits.NormalizePageNumber(3).Should().Be(3);
            PaginationLimits.NormalizePageSize(0).Should().Be(PaginationLimits.DefaultPageSize);
            PaginationLimits.NormalizePageSize(500).Should().Be(PaginationLimits.MaxPageSize);
            PaginationLimits.NormalizePageSize(25).Should().Be(25);
        }
    }
}
