using Conciliaciones.Aplicacion.Features.Conciliation.Commands.CreateParametrizacionFull;
using Conciliaciones.Aplicacion.Ports.Interfaces;
using Conciliaciones.Dominio.DTOs.Requests.Conciliation;
using Conciliaciones.Dominio.DTOs.Response.Conciliation;
using Conciliaciones.Dominio.Envoltorios;
using Conciliaciones.Pruebas.Support;
using FluentAssertions;
using Moq;
using NUnit.Framework;

namespace Conciliaciones.Pruebas.Conciliaciones.Aplicacion.NUnit
{
    [TestFixture]
    public class CreateParametrizacionFullHandlerTests : NUnitTestBase
    {
        private Mock<IParametrizacionApplicationService> _service = null!;
        private CreateParametrizacionFullCommandHandler _sut = null!;

        [SetUp]
        public void SetUp()
        {
            _service = FreezeMock<IParametrizacionApplicationService>();
            _sut = new CreateParametrizacionFullCommandHandler(_service.Object);
        }

        [Test]
        public void Ctor_NullService_Throws()
        {
            Action act = () => _ = new CreateParametrizacionFullCommandHandler(null!);
            act.Should().Throw<ArgumentNullException>();
        }

        [Test]
        public async Task Handle_NullCommand_Throws()
        {
            Func<Task> act = async () => await _sut.Handle(null!, CancellationToken.None);
            await act.Should().ThrowAsync<ArgumentNullException>();
        }

        [Test]
        public async Task Handle_NullRequest_Throws()
        {
            Func<Task> act = async () => await _sut.Handle(new CreateParametrizacionFullCommand(null!), CancellationToken.None);
            await act.Should().ThrowAsync<ArgumentNullException>();
        }

        [Test]
        public async Task Handle_HappyPath_MapsResponse_AndPassesToken()
        {
            // Arrange
            using CancellationTokenSource cts = new();
            CreateParametrizacionRequest request = new() { proyecto = "P", proceso = "R", subproceso = "S" };
            CreateParametrizacionResponse data = new("idP", "idR", "idS", "param", "ok");
            _service.Setup(s => s.CreateAsync(request, cts.Token)).ReturnsAsync(data);

            // Act
            Answers<CreateParametrizacionFullResponse> result =
                await _sut.Handle(new CreateParametrizacionFullCommand(request), cts.Token);

            // Assert
            result.Resultado.Should().NotBeNull();
            result.Resultado!.IdProyecto.Should().Be("idP");
            result.Resultado.ParametrizacionId.Should().Be("param");
            _service.Verify(s => s.CreateAsync(request, cts.Token), Times.Once);
        }

        [Test]
        public async Task Handle_WhenCancelled_PropagatesOperationCanceled()
        {
            using CancellationTokenSource cts = new();
            cts.Cancel();
            CreateParametrizacionRequest request = new();
            _service.Setup(s => s.CreateAsync(request, cts.Token))
                .ThrowsAsync(new OperationCanceledException(cts.Token));

            Func<Task> act = async () => await _sut.Handle(new CreateParametrizacionFullCommand(request), cts.Token);
            await act.Should().ThrowAsync<OperationCanceledException>();
        }
    }
}
