using Conciliaciones.Aplicacion.Features.Conciliation.Querys.GetConciliationsByIdSp;
using Conciliaciones.Dominio.DTOs.Response.Conciliation;
using Conciliaciones.Dominio.Envoltorios;
using Moq;
using Conciliaciones.Aplicacion.Ports.Interfaces;

namespace Conciliaciones.Pruebas.Conciliaciones.Aplicacion.Features.Conciliation.Querys.GetConciliationsByIdSp
{
    [TestClass]
    public class GetConciliationsByIdSpHandlerTests
    {
        private readonly Mock<IConciliationQueryService> _conciliationsRepositoryMock;

        public GetConciliationsByIdSpHandlerTests()
        {
            _conciliationsRepositoryMock = new Mock<IConciliationQueryService>();
        }

        [TestMethod]
        public void Constructor_ConRepositorioNulo_LanzaArgumentNullException()
        {
            // Arrange & Act & Assert
            Assert.ThrowsExactly<ArgumentNullException>(() => new GetConciliationsByIdSpHandler(null!));
        }

        [TestMethod]
        public async Task Handle_ConSolicitudNula_LanzaArgumentNullException()
        {
            // Arrange
            GetConciliationsByIdSpHandler handler = new GetConciliationsByIdSpHandler(_conciliationsRepositoryMock.Object);

            // Act & Assert
            await Assert.ThrowsExactlyAsync<ArgumentNullException>(() =>
                handler.Handle(null!, CancellationToken.None));
        }

        [TestMethod]
        public async Task Handle_ConsultaExitosa_RetornaAnswersConListaDeBloques()
        {
            // Arrange
            string reglaId = "d75f5195-9109-4aea-a617-9c114dbc0443";

            List<BlockUiDto> blocksMock = new List<BlockUiDto>
            {
                new BlockUiDto("discrepancias-table", "table", "Diferencias detectadas", null!)
            };

            ColumsDetailsDto columsDetailsDtoMock = new ColumsDetailsDto
            (
                blocksMock
            );

            _conciliationsRepositoryMock
                .Setup(r => r.GetConciliationByIdSp(reglaId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(columsDetailsDtoMock);

            GetConciliationsByIdSpQuery query = new GetConciliationsByIdSpQuery(reglaId);
            GetConciliationsByIdSpHandler handler = new GetConciliationsByIdSpHandler(_conciliationsRepositoryMock.Object);

            // Act
            Answers<GetConciliationsByIdSpResponse> result = await handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsNotNull(result.Resultado);
            Assert.IsNotNull(result.Resultado.blocks);
            Assert.AreEqual(1, result?.Resultado.blocks.Count);
            Assert.AreEqual("discrepancias-table", result?.Resultado.blocks[0].Id);

            _conciliationsRepositoryMock.Verify(r => r.GetConciliationByIdSp(reglaId, It.IsAny<CancellationToken>()), Times.Once);
        }
    }
}