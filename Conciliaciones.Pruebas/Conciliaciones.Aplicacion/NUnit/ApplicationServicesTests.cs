using Conciliaciones.Aplicacion.Services;
using Conciliaciones.Dominio.DTOs.Requests.Conciliation;
using Conciliaciones.Dominio.DTOs.Response.Conciliation;
using Conciliaciones.Dominio.Ports.Interfaces.Conciliations;
using Conciliaciones.Pruebas.Support;
using FluentAssertions;
using Moq;
using NUnit.Framework;

namespace Conciliaciones.Pruebas.Conciliaciones.Aplicacion.NUnit
{
    [TestFixture]
    public class ApplicationServicesTests : NUnitTestBase
    {
        [Test]
        public void ConciliationQueryService_Ctor_Null_Throws()
        {
            Action act = () => _ = new ConciliationQueryService(null!);
            act.Should().Throw<ArgumentNullException>().WithParameterName("queryPort");
        }

        [Test]
        public async Task ConciliationQueryService_Delegates_AndHonorsCancellationToken()
        {
            // Arrange
            Mock<IConciliationQueryPort> port = new();
            using CancellationTokenSource cts = new();
            ColumsDetailsDto columns = new(new List<BlockUiDto>());
            PagedRowsDto rows = new(0, 0, 0, 0, 1, 10, new List<Dictionary<string, object?>>());
            List<FuenteDetalleDto> fuentes = new();

            port.Setup(p => p.GetConciliationByIdSp("regla", cts.Token)).ReturnsAsync(columns);
            port.Setup(p => p.GetConciliationRowsAsync("regla", 1, 10, cts.Token)).ReturnsAsync(rows);
            port.Setup(p => p.GetConciliationFuentesDetailAsync("c1", cts.Token)).ReturnsAsync(fuentes);

            ConciliationQueryService sut = new(port.Object);

            // Act
            ColumsDetailsDto c = await sut.GetConciliationByIdSp("regla", cts.Token);
            PagedRowsDto r = await sut.GetConciliationRowsAsync("regla", 1, 10, cts.Token);
            List<FuenteDetalleDto> f = await sut.GetConciliationFuentesDetailAsync("c1", cts.Token);

            // Assert
            c.Should().BeSameAs(columns);
            r.Should().BeSameAs(rows);
            f.Should().BeSameAs(fuentes);
            port.Verify(p => p.GetConciliationByIdSp("regla", cts.Token), Times.Once);
            port.Verify(p => p.GetConciliationRowsAsync("regla", 1, 10, cts.Token), Times.Once);
            port.Verify(p => p.GetConciliationFuentesDetailAsync("c1", cts.Token), Times.Once);
        }

        [Test]
        public void ConciliationStateService_Ctor_Null_Throws()
        {
            Action act = () => _ = new ConciliationStateService(null!);
            act.Should().Throw<ArgumentNullException>();
        }

        [Test]
        public async Task ConciliationStateService_Update_EmptyList_DelegatesToken()
        {
            Mock<IConciliationStatePort> port = FreezeMock<IConciliationStatePort>();
            using CancellationTokenSource cts = new();
            StatusUpdateConciliation status = new(new List<Guid>(), new List<Guid>(), 0);
            List<Guid> ids = new();

            port.Setup(p => p.UpdateEstadoConciliacionAsync(ids, cts.Token)).ReturnsAsync(status);
            ConciliationStateService sut = new(port.Object);

            StatusUpdateConciliation result = await sut.UpdateEstadoAsync(ids, cts.Token);

            result.Should().BeSameAs(status);
            port.Verify(p => p.UpdateEstadoConciliacionAsync(ids, cts.Token), Times.Once);
        }

        [Test]
        public void ParametrizacionApplicationService_Ctor_Null_Throws()
        {
            Action act = () => _ = new ParametrizacionApplicationService(null!);
            act.Should().Throw<ArgumentNullException>();
        }

        [Test]
        public async Task ParametrizacionApplicationService_Create_NullRequest_StillDelegates()
        {
            Mock<IParametrizacionWritePort> port = FreezeMock<IParametrizacionWritePort>();
            CreateParametrizacionResponse response = new("p", "pr", "s", "id", "ok");
            port.Setup(p => p.CreateParametrizacionAsync(null!, It.IsAny<CancellationToken>()))
                .ReturnsAsync(response);

            ParametrizacionApplicationService sut = new(port.Object);
            CreateParametrizacionResponse result = await sut.CreateAsync(null!, CancellationToken.None);

            result.Should().BeSameAs(response);
        }
    }
}
