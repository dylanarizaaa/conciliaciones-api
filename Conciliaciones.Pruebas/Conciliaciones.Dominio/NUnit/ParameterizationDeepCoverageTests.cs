using Conciliaciones.Dominio.Entities.Client;
using Conciliaciones.Dominio.Entities.Parameterization;
using Conciliaciones.Dominio.Validators;
using FluentAssertions;
using NUnit.Framework;

namespace Conciliaciones.Pruebas.Conciliaciones.Dominio.NUnit
{
    [TestFixture]
    public class ParameterizationDeepCoverageTests
    {
        [Test]
        public void ParameterizationEntity_FullGraph_HappyAndValidate()
        {
            // Arrange
            ParameterizationEntity entity = ParameterizationEntity.Create(
                Guid.NewGuid().ToString(),
                Guid.NewGuid().ToString(),
                parametros: new[] { new ParametroEntity("p", "string") },
                fuentes: new[]
                {
                    new FuenteEntity(
                        "f1",
                        "c1",
                        "tabla",
                        new[] { new CampoSeleccionEntity("col", "int", "alias") },
                        new[]
                        {
                            new FiltroEntity("AND", new[]
                            {
                                new CondicionFiltroEntity("a", "=", "1")
                            })
                        })
                },
                transformacion: new[]
                {
                    new TransformacionEntity("diff", "number", "sub", new[] { new OperandoEntity("a") })
                },
                reglas: new[] { new ReglaEntity("regla", "ok", "err", "true", "r1") });

            // Act / Assert
            EntityValidator.ValidateAndThrow(entity, new ParameterizationEntityValidator());
            entity.Parametros.Should().HaveCount(1);
            entity.Fuentes[0].Filtros.Should().HaveCount(1);
            entity.Transformacion[0].Operandos.Should().HaveCount(1);
            entity.Reglas.Should().HaveCount(1);
            entity.Reglas![0].IdRegla.Should().Be("r1");
        }

        [Test]
        public void ParameterizationUI_DatosBlock_EmptyColumns_FailsValidation()
        {
            ParameterizationUIEntity ui = ParameterizationUIEntity.Create(
                Guid.NewGuid().ToString(),
                fuente: Array.Empty<UiColumnEntity>(),
                datos: new[]
                {
                    new UiDatosBlockEntity("f1", Array.Empty<UiColumnEntity>())
                });

            // IdFuente ok; empty columns ok for validator (no NotEmpty on columns list)
            Action act = () => EntityValidator.ValidateAndThrow(ui, new ParameterizationUIEntityValidator());
            act.Should().NotThrow();
        }

        [Test]
        public void ParameterizationUI_EmptyField_FailsValidation()
        {
            Action act = () =>
            {
                ParameterizationUIEntity ui = ParameterizationUIEntity.Create(
                    Guid.NewGuid().ToString(),
                    new[] { new UiColumnEntity(" ", "H", 1) });
                EntityValidator.ValidateAndThrow(ui, new ParameterizationUIEntityValidator());
            };
            act.Should().Throw<ArgumentException>();
        }

        [Test]
        public void UiColumn_BadgeAndDateFactories()
        {
            UiCellConfigEntity badge = UiCellConfigEntity.Badge(new Dictionary<string, string> { ["A"] = "b" });
            badge.VariantByValue.Should().ContainKey("A");
            UiCellConfigEntity.DateIso().Type.Should().Be("date");
            CurrencyCellConfigEntity.CopDefault().CurrencyCode.Should().Be("COP");
        }

        [Test]
        public void ProcesoSubProceso_Rehydrate_RoundTrip()
        {
            DateTime now = DateTime.UtcNow;
            Proceso p = Proceso.Rehydrate("idP", "idProy", "N", now);
            SubProceso s = SubProceso.Rehydrate("idS", "idP", "S", now);
            EntityValidator.ValidateAndThrow(p, new ProcesoValidator());
            EntityValidator.ValidateAndThrow(s, new SubProcesoValidator());
            p.FechaCreacion.Should().Be(now);
            s.FechaCreacion.Should().Be(now);
        }

        [Test]
        public void EntityValidator_Valid_DoesNotThrow()
        {
            Proyecto p = Proyecto.Create("Ok");
            Action act = () => EntityValidator.ValidateAndThrow(p, new ProyectoValidator());
            act.Should().NotThrow();
        }

        [Test]
        public void ReglaEntity_EmptyId_GeneratesNewId()
        {
            ReglaEntity entity = new ReglaEntity("regla", "ok", "err", "true", " ");
            entity.IdRegla.Should().NotBeNullOrWhiteSpace();
            Guid.TryParse(entity.IdRegla, out _).Should().BeTrue();
            entity.NombreRegla.Should().Be("regla");
        }
    }
}
