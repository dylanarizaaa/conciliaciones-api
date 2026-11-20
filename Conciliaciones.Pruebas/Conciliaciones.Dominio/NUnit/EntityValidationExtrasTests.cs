using Conciliaciones.Dominio.Entities.Common;
using Conciliaciones.Dominio.Entities.Conciliation;
using Conciliaciones.Dominio.Utils;
using FluentAssertions;
using NUnit.Framework;

namespace Conciliaciones.Pruebas.Conciliaciones.Dominio.NUnit
{
    [TestFixture]
    public class EntityValidationExtrasTests
    {
        [Test]
        public void OptionalTrim_NullAndWhitespace_ReturnsEmpty()
        {
            EntityValidation.OptionalTrim(null).Should().BeEmpty();
            EntityValidation.OptionalTrim("  ").Should().BeEmpty();
            EntityValidation.OptionalTrim(" x ").Should().Be("x");
        }

        [Test]
        public void ToIso_AndFromIso_RoundTrip()
        {
            DateTime utc = DateTime.UtcNow;
            string iso = EntityValidation.ToIso(utc);
            DateTime parsed = EntityValidation.FromIsoOrUtcNow(iso);
            parsed.Should().BeCloseTo(utc, TimeSpan.FromSeconds(1));
        }

        [Test]
        public void NewId_IsGuid()
        {
            Guid.TryParse(EntityValidation.NewId(), out _).Should().BeTrue();
        }

        [Test]
        public void Conciliacion_WithFuentesAndRegla_Rehydrates()
        {
            ConciliacionEntity entity = ConciliacionEntity.Rehydrate(
                Guid.NewGuid().ToString(),
                "param",
                ConciliacionEstados.con_novedad,
                new Dictionary<string, object> { ["m"] = 1m },
                new[]
                {
                    new ResultadoFuenteEntity("f", "c", new[]
                    {
                        (IReadOnlyDictionary<string, object>)new Dictionary<string, object> { ["k"] = "v" }
                    })
                },
                new ResultadoReglaEntity("reglaX"));

            entity.EsFallidaOError.Should().BeTrue();
            entity.ResultadoFuentes![0].Data.Should().HaveCount(1);
            entity.ResultadoRegla!.NombreRegla.Should().Be("reglaX");
        }

        [Test]
        public void ConstantData_FechaActual_IsSet()
        {
            ConstantData.FechaActual.ToString().Should().NotBeNullOrWhiteSpace();
        }

        [Test]
        public void ConciliacionEstados_FallidosOError_ContainsExpected()
        {
            ConciliacionEstados.FallidosOError.Should().Contain(ConciliacionEstados.con_novedad);
            ConciliacionEstados.FallidosOErrorComparacion.Should().Contain("con_novedad");
        }
    }
}
