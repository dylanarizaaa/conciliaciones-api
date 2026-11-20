using Conciliaciones.Aplicacion.Features.Conciliation.Querys.GetConciliationFuentesDetail;
using Conciliaciones.Dominio.DTOs.Response.Conciliation;
using Conciliaciones.Dominio.Envoltorios;
using Moq;
using Conciliaciones.Aplicacion.Ports.Interfaces;

namespace Conciliaciones.Pruebas.Conciliaciones.Aplicacion.Features.Conciliation.Querys.GetConciliationFuentesDetail
{
    [TestClass]
    public class GetConciliationFuentesDetailHandlerTests
    {
        private readonly Mock<IConciliationQueryService> _conciliationRepositoryMock;

        public GetConciliationFuentesDetailHandlerTests()
        {
            _conciliationRepositoryMock = new Mock<IConciliationQueryService>();
        }

        [TestMethod]
        public void Constructor_ConRepositorioNulo_LanzaArgumentNullException()
        {
            // Arrange & Act & Assert
            Assert.ThrowsExactly<ArgumentNullException>(() => new GetConciliationFuentesDetailHandler(null!));
        }

        [TestMethod]
        public async Task Handle_ConSolicitudNula_LanzaArgumentNullException()
        {
            // Arrange
            GetConciliationFuentesDetailHandler handler = new GetConciliationFuentesDetailHandler(_conciliationRepositoryMock.Object);

            // Act & Assert
            await Assert.ThrowsExactlyAsync<ArgumentNullException>(() =>
                handler.Handle(null!, CancellationToken.None));
        }

        [TestMethod]
        public async Task Handle_ConsultaExitosa_RetornaAnswersConListaDeFuentes()
        {
            // Arrange
            string idConciliacionBusqueda = "1a2b3c4d-5e6f-7a8b-9c0d-1e2f3a4b5c6d";

            List<DatoDetalleDto> datosFuente = new List<DatoDetalleDto>
            {
                new DatoDetalleDto("Factura Id", "number", 2)
            };

            List<FuenteDetalleDto> fuentesMock = new List<FuenteDetalleDto>
            {
                new FuenteDetalleDto("fuenteFacturas", datosFuente)
            };

            _conciliationRepositoryMock
                .Setup(r => r.GetConciliationFuentesDetailAsync(idConciliacionBusqueda, It.IsAny<CancellationToken>()))
                .ReturnsAsync(fuentesMock);

            GetConciliationFuentesDetailQuery query = new GetConciliationFuentesDetailQuery(idConciliacionBusqueda);
            GetConciliationFuentesDetailHandler handler = new GetConciliationFuentesDetailHandler(_conciliationRepositoryMock.Object);

            // Act
            Answers<GetConciliationFuentesDetailResponse> result = await handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsNotNull(result.Resultado);
            Assert.IsNotNull(result.Resultado.data);
            Assert.AreEqual(1, result?.Resultado.data.Count);
            Assert.AreEqual("fuenteFacturas", result?.Resultado.data[0].Nombre);

            _conciliationRepositoryMock.Verify(r => r.GetConciliationFuentesDetailAsync(idConciliacionBusqueda, It.IsAny<CancellationToken>()), Times.Once);
        }
    }
}