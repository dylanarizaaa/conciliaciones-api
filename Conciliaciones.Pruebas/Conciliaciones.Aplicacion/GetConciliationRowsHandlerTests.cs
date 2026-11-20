using Conciliaciones.Aplicacion.Features.Conciliation.Querys.GetConciliationRows;
using Conciliaciones.Dominio.DTOs.Response.Conciliation;
using Conciliaciones.Dominio.Envoltorios;
using Moq;
using Conciliaciones.Aplicacion.Ports.Interfaces;

namespace Conciliaciones.Pruebas.Conciliaciones.Aplicacion.Features.Conciliation.Querys.GetConciliationRows
{
    [TestClass]
    public class GetConciliationRowsHandlerTests
    {
        private readonly Mock<IConciliationQueryService> _conciliationRepositoryMock;

        public GetConciliationRowsHandlerTests()
        {
            _conciliationRepositoryMock = new Mock<IConciliationQueryService>();
        }

        [TestMethod]
        public void Constructor_ConRepositorioNulo_LanzaArgumentNullException()
        {
            // Arrange & Act & Assert
            Assert.ThrowsExactly<ArgumentNullException>(() => new GetConciliationRowsHandler(null!));
        }

        [TestMethod]
        public async Task Handle_ConSolicitudNula_LanzaArgumentNullException()
        {
            // Arrange
            GetConciliationRowsHandler handler = new GetConciliationRowsHandler(_conciliationRepositoryMock.Object);

            // Act & Assert
            await Assert.ThrowsExactlyAsync<ArgumentNullException>(() =>
                handler.Handle(null!, CancellationToken.None));
        }

        [TestMethod]
        public async Task Handle_PaginacionInvalida_AjustaParametrosYRetornaRespuestaPaginada()
        {
            // Arrange
            string reglaId = "regla-monto-exacto";
            int pageNumberInvalido = 0; // Se debe ajustar a 1
            int pageSizeInvalido = -5;  // Se debe ajustar a 10

            List<Dictionary<string, object?>> rowsMock = new List<Dictionary<string, object?>>
            {
                new Dictionary<string, object?>
                {
                    { "conciliacionId", "12345678-1234-1234-1234-123456789012" },
                    { "MontoTotal", 150000.00m },
                    { "estadoConciliacion", "fallido" }
                }
            };

            PagedRowsDto pagedRowsDtoMock = new PagedRowsDto(
                CasosTotales: 1,
                MontoAfectado: 150000.00m,
                TotalCount: 1,
                TotalPages: 1,
                PageNumber: 1,
                PageSize: 10,
                Rows: rowsMock
            );

            _conciliationRepositoryMock
                .Setup(r => r.GetConciliationRowsAsync(reglaId, 1, 10, It.IsAny<CancellationToken>()))
                .ReturnsAsync(pagedRowsDtoMock);

            GetConciliationRowsQuery query = new GetConciliationRowsQuery(reglaId, pageNumberInvalido, pageSizeInvalido);
            GetConciliationRowsHandler handler = new GetConciliationRowsHandler(_conciliationRepositoryMock.Object);

            // Act
            Answers<GetConciliationRowsResponse> result = await handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsNotNull(result.Resultado);
            Assert.AreEqual(1, result.Resultado.casosTotales);
            Assert.AreEqual(150000.00m, result.Resultado.montoAfectado);
            Assert.AreEqual(1, result.Resultado.pageNumber);
            Assert.AreEqual(10, result.Resultado.pageSize);
            Assert.AreEqual(1, result?.Resultado.data.Count);

            _conciliationRepositoryMock.Verify(r => r.GetConciliationRowsAsync(reglaId, 1, 10, It.IsAny<CancellationToken>()), Times.Once);
        }

        [TestMethod]
        public async Task Handle_ConsultaExitosa_RetornaMapeoCorrecto()
        {
            // Arrange
            string reglaId = "regla-validacion-fecha";
            int pageNumber = 2;
            int pageSize = 10;

            List<Dictionary<string, object?>> rowsMock = new List<Dictionary<string, object?>>
            {
                new Dictionary<string, object?> { { "conciliacionId", Guid.NewGuid().ToString() } },
                new Dictionary<string, object?> { { "conciliacionId", Guid.NewGuid().ToString() } }
            };

            PagedRowsDto pagedRowsDtoMock = new PagedRowsDto(
                CasosTotales: 15,
                MontoAfectado: 500000.50m,
                TotalCount: 15,
                TotalPages: 2,
                PageNumber: 2,
                PageSize: 10,
                Rows: rowsMock
            );

            _conciliationRepositoryMock
                .Setup(r => r.GetConciliationRowsAsync(reglaId, pageNumber, pageSize, It.IsAny<CancellationToken>()))
                .ReturnsAsync(pagedRowsDtoMock);

            GetConciliationRowsQuery query = new GetConciliationRowsQuery(reglaId, pageNumber, pageSize);
            GetConciliationRowsHandler handler = new GetConciliationRowsHandler(_conciliationRepositoryMock.Object);

            // Act
            Answers<GetConciliationRowsResponse> result = await handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsNotNull(result.Resultado);
            Assert.AreEqual(15, result.Resultado.casosTotales);
            Assert.AreEqual(500000.50m, result.Resultado.montoAfectado);
            Assert.AreEqual(2, result.Resultado.totalPages);
            Assert.AreEqual(2, result?.Resultado.data.Count);

            _conciliationRepositoryMock.Verify(r => r.GetConciliationRowsAsync(reglaId, pageNumber, pageSize, It.IsAny<CancellationToken>()), Times.Once);
        }
    }
}