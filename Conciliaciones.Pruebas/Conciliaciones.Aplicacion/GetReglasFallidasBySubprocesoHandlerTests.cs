using Conciliaciones.Aplicacion.Features.SubProcess.Querys.GetReglasFallidasBySubproceso;
using Conciliaciones.Dominio.DTOs.Response.SubProcess;
using Conciliaciones.Dominio.Envoltorios;
using Conciliaciones.Dominio.Ports.Interfaces.SubProcess;
using Moq;

namespace Conciliaciones.Pruebas.Conciliaciones.Aplicacion.Features.SubProcess.Querys.GetReglasFallidasBySubproceso
{
    [TestClass]
    public class GetReglasFallidasBySubprocesoHandlerTests
    {
        private readonly Mock<ISubProcessRepository> _subProcessRepositoryMock;

        public GetReglasFallidasBySubprocesoHandlerTests()
        {
            _subProcessRepositoryMock = new Mock<ISubProcessRepository>();
        }

        [TestMethod]
        public void Constructor_ConRepositorioNulo_LanzaArgumentNullException()
        {
            // Arrange & Act & Assert
            Assert.ThrowsExactly<ArgumentNullException>(() => new GetReglasFallidasBySubprocesoHandler(null!));
        }

        [TestMethod]
        public async Task Handle_ConSolicitudNula_LanzaArgumentNullException()
        {
            // Arrange
            GetReglasFallidasBySubprocesoHandler handler = new GetReglasFallidasBySubprocesoHandler(_subProcessRepositoryMock.Object);

            // Act & Assert
            await Assert.ThrowsExactlyAsync<ArgumentNullException>(() =>
                handler.Handle(null!, CancellationToken.None));
        }

        [TestMethod]
        public async Task Handle_ConsultaExitosa_RetornaPaginacionYDataCorrecta()
        {
            // Arrange
            string subProcesoId = "SP-001";
            int pageNumber = 1;
            int pageSize = 10;

            List<FailedRuleDto> failedRulesMock = new List<FailedRuleDto>
            {
                new FailedRuleDto("id-1", "ValidacionMontoExacto", 15, 85000.00m),
                new FailedRuleDto("id-2", "ValidacionFechaEmision", 10, 40000.50m)
            };

            SubprocesoReglasResumenDto resumenDtoMock = new SubprocesoReglasResumenDto(
                CasosTotales: 25,
                MontoAfectado: 125000.50m,
                PageNumber: pageNumber,
                PageSize: pageSize,
                TotalPages: 3,
                Data: failedRulesMock
            );

            _subProcessRepositoryMock
                .Setup(r => r.GetReglasFallidasBySubprocesoIdAsync(subProcesoId, pageNumber, pageSize, It.IsAny<CancellationToken>()))
                .ReturnsAsync(resumenDtoMock);

            GetReglasFallidasBySubprocesoQuery query = new GetReglasFallidasBySubprocesoQuery(subProcesoId, pageNumber, pageSize);
            GetReglasFallidasBySubprocesoHandler handler = new GetReglasFallidasBySubprocesoHandler(_subProcessRepositoryMock.Object);

            // Act
            Answers<GetReglasFallidasBySubprocesoResponse> result = await handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsNotNull(result.Resultado);
            Assert.AreEqual(25L, result.Resultado.casosTotales);
            Assert.AreEqual(125000.50m, result.Resultado.montoAfectado);
            Assert.AreEqual(1, result.Resultado.pageNumber);
            Assert.AreEqual(10, result.Resultado.pageSize);
            Assert.AreEqual(3L, result.Resultado.totalPages);
            Assert.AreEqual(2, result?.Resultado.data.Count);
            Assert.AreEqual("ValidacionMontoExacto", result?.Resultado.data[0].nombreRegla);

            _subProcessRepositoryMock.Verify(r => r.GetReglasFallidasBySubprocesoIdAsync(subProcesoId, pageNumber, pageSize, It.IsAny<CancellationToken>()), Times.Once);
        }

        [TestMethod]
        public async Task Handle_PaginacionInvalida_SanitizaParametrosYEjecutaConsulta()
        {
            // Arrange
            string subProcesoId = "SP-002";
            int pageNumberInvalido = -1; // Se sanitiza a 1
            int pageSizeInvalido = 0;   // Se sanitiza a 10

            SubprocesoReglasResumenDto resumenDtoMock = new SubprocesoReglasResumenDto(
                CasosTotales: 0,
                MontoAfectado: 0m,
                PageNumber: 1,
                PageSize: 10,
                TotalPages: 0,
                Data: new List<FailedRuleDto>()
            );

            _subProcessRepositoryMock
                .Setup(r => r.GetReglasFallidasBySubprocesoIdAsync(subProcesoId, 1, 10, It.IsAny<CancellationToken>()))
                .ReturnsAsync(resumenDtoMock);

            GetReglasFallidasBySubprocesoQuery query = new GetReglasFallidasBySubprocesoQuery(subProcesoId, pageNumberInvalido, pageSizeInvalido);
            GetReglasFallidasBySubprocesoHandler handler = new GetReglasFallidasBySubprocesoHandler(_subProcessRepositoryMock.Object);

            // Act
            Answers<GetReglasFallidasBySubprocesoResponse> result = await handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsNotNull(result.Resultado);
            Assert.AreEqual(1, result.Resultado.pageNumber);
            Assert.AreEqual(10, result.Resultado.pageSize);
            Assert.AreEqual(0, result?.Resultado.data.Count);

            _subProcessRepositoryMock.Verify(r => r.GetReglasFallidasBySubprocesoIdAsync(subProcesoId, 1, 10, It.IsAny<CancellationToken>()), Times.Once);
        }
    }
}