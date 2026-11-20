using Conciliaciones.Dominio.Ports.Interfaces.Hierarchy;
using Conciliaciones.Infraestructura.Persistence.Repositories;
using Moq;

namespace Conciliaciones.Pruebas.Conciliaciones.Infraestructura.Persistence.Repositories
{
    [TestClass]
    public class HierarchyWriteRepositoryTests
    {
        private Mock<IGetOrCreatePort> _getOrCreateMock = null!;
        private HierarchyWriteRepository _repository = null!;

        [TestInitialize]
        public void Setup()
        {
            _getOrCreateMock = new Mock<IGetOrCreatePort>();
            _repository = new HierarchyWriteRepository(_getOrCreateMock.Object);
        }

        [TestMethod]
        public async Task ResolveOrCreateProyectoAsync_DelegaAGetOrCreateConKindProyecto()
        {
            _getOrCreateMock
                .Setup(p => p.ResolveOrCreateAsync(HierarchyEntityKind.Proyecto, "P1", null, It.IsAny<CancellationToken>()))
                .ReturnsAsync("ID-P");

            string result = await _repository.ResolveOrCreateProyectoAsync("P1", CancellationToken.None);

            Assert.AreEqual("ID-P", result);
            _getOrCreateMock.Verify(
                p => p.ResolveOrCreateAsync(HierarchyEntityKind.Proyecto, "P1", null, It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [TestMethod]
        public async Task ResolveOrCreateProcesoAsync_DelegaConParentIdProyecto()
        {
            _getOrCreateMock
                .Setup(p => p.ResolveOrCreateAsync(HierarchyEntityKind.Proceso, "Proc", "PROY-1", It.IsAny<CancellationToken>()))
                .ReturnsAsync("ID-PROC");

            string result = await _repository.ResolveOrCreateProcesoAsync("Proc", "PROY-1", CancellationToken.None);

            Assert.AreEqual("ID-PROC", result);
        }

        [TestMethod]
        public async Task ResolveOrCreateSubProcesoAsync_DelegaConParentIdProceso()
        {
            _getOrCreateMock
                .Setup(p => p.ResolveOrCreateAsync(HierarchyEntityKind.SubProceso, "Sub", "PROC-1", It.IsAny<CancellationToken>()))
                .ReturnsAsync("ID-SUB");

            string result = await _repository.ResolveOrCreateSubProcesoAsync("Sub", "PROC-1", CancellationToken.None);

            Assert.AreEqual("ID-SUB", result);
        }
    }
}
