using AutoFixture;
using AutoFixture.AutoMoq;
using Moq;

namespace Conciliaciones.Pruebas.Support
{
    /// <summary>Base AAA: Fixture + AutoMoq compartidos (evita setup duplicado).</summary>
    public abstract class NUnitTestBase
    {
        protected IFixture Fixture { get; } = new Fixture().Customize(new AutoMoqCustomization { ConfigureMembers = true });

        protected Mock<T> FreezeMock<T>() where T : class => Fixture.Freeze<Mock<T>>();
    }
}
