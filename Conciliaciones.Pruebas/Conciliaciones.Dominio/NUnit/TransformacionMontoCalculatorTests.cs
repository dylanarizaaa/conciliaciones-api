using Conciliaciones.Dominio.Services;
using FluentAssertions;
using NUnit.Framework;

namespace Conciliaciones.Pruebas.Conciliaciones.Dominio.NUnit
{
    [TestFixture]
    public class TransformacionMontoCalculatorTests
    {
        private TransformacionMontoCalculator _sut = null!;

        [SetUp]
        public void SetUp() => _sut = new TransformacionMontoCalculator();

        [Test]
        public void Sum_HappyPath_ReturnsTotal()
        {
            // Arrange
            decimal[] valores = { 10.5m, 20m, -5m };

            // Act
            decimal result = _sut.Sum(valores);

            // Assert
            result.Should().Be(25.5m);
        }

        [Test]
        public void Sum_NullCollection_ReturnsZero()
        {
            _sut.Sum(null!).Should().Be(0m);
        }

        [Test]
        public void Sum_EmptyCollection_ReturnsZero()
        {
            _sut.Sum(Array.Empty<decimal>()).Should().Be(0m);
        }

        [Test]
        public void ParseAmount_Null_ReturnsZero()
        {
            _sut.ParseAmount(null).Should().Be(0m);
        }

        [Test]
        public void ParseAmount_Decimal_ReturnsSame()
        {
            _sut.ParseAmount(123.45m).Should().Be(123.45m);
        }

        [Test]
        public void ParseAmount_InvariantString_Parses()
        {
            _sut.ParseAmount("99.99").Should().Be(99.99m);
        }

        [Test]
        public void ParseAmount_InvalidString_ReturnsZero()
        {
            _sut.ParseAmount("not-a-number").Should().Be(0m);
        }

        [Test]
        public void ParseAmount_ConvertibleInt_Parses()
        {
            _sut.ParseAmount(42).Should().Be(42m);
        }
    }
}
