using Conciliaciones.Dominio.Ports.Interfaces.Hierarchy;
using Conciliaciones.Dominio.Services;
using Conciliaciones.Infraestructura.Persistence.Hierarchy;
using Conciliaciones.Infraestructura.Ports.Interfaces.Persistence;
using Moq;

namespace Conciliaciones.Pruebas.Conciliaciones.Infraestructura.Persistence.Hierarchy
{
    [TestClass]
    public class GetOrCreateRepositoryTests
    {
        private Mock<IHierarchyEntityStrategy> _proyectoStrategy = null!;
        private GetOrCreateRepository _repository = null!;

        [TestInitialize]
        public void Setup()
        {
            _proyectoStrategy = CreateStrategyMock(HierarchyEntityKind.Proyecto);
            Mock<IHierarchyEntityStrategy> procesoStrategy = CreateStrategyMock(HierarchyEntityKind.Proceso);
            Mock<IHierarchyEntityStrategy> subProcesoStrategy = CreateStrategyMock(HierarchyEntityKind.SubProceso);

            _repository = new GetOrCreateRepository(
                HierarchyIdentityPolicy.Instance,
                new[] { _proyectoStrategy.Object, procesoStrategy.Object, subProcesoStrategy.Object });
        }

        [TestMethod]
        public async Task ResolveOrCreateAsync_CuandoCandidateEsGuidYExiste_RetornaCandidate()
        {
            string existingId = Guid.NewGuid().ToString();
            _proyectoStrategy
                .Setup(s => s.ExistsByIdAsync(existingId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            string result = await _repository.ResolveOrCreateAsync(
                HierarchyEntityKind.Proyecto,
                existingId,
                parentId: null,
                CancellationToken.None);

            Assert.AreEqual(existingId, result);
            _proyectoStrategy.Verify(
                s => s.ExistsByIdAsync(existingId, It.IsAny<CancellationToken>()),
                Times.Once);
            _proyectoStrategy.Verify(
                s => s.FindExistingIdAsync(It.IsAny<string>(), It.IsAny<string?>(), It.IsAny<CancellationToken>()),
                Times.Never);
            _proyectoStrategy.Verify(
                s => s.CreateAsync(It.IsAny<string>(), It.IsAny<string?>(), It.IsAny<CancellationToken>()),
                Times.Never);
        }

        [TestMethod]
        public async Task ResolveOrCreateAsync_CuandoCandidateEsGuidYNoExiste_LanzaKeyNotFoundException()
        {
            string missingId = Guid.NewGuid().ToString();
            _proyectoStrategy
                .Setup(s => s.ExistsByIdAsync(missingId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(false);

            try
            {
                await _repository.ResolveOrCreateAsync(
                    HierarchyEntityKind.Proyecto,
                    missingId,
                    parentId: null,
                    CancellationToken.None);
                Assert.Fail("Se esperaba KeyNotFoundException.");
            }
            catch (KeyNotFoundException)
            {
            }
        }

        [TestMethod]
        public async Task ResolveOrCreateAsync_CuandoExistePorNombre_RetornaIdExistenteSinCrear()
        {
            _proyectoStrategy
                .Setup(s => s.FindExistingIdAsync("Proyecto A", null, It.IsAny<CancellationToken>()))
                .ReturnsAsync("PROY-EXISTENTE");

            string result = await _repository.ResolveOrCreateAsync(
                HierarchyEntityKind.Proyecto,
                "Proyecto A",
                parentId: null,
                CancellationToken.None);

            Assert.AreEqual("PROY-EXISTENTE", result);
            _proyectoStrategy.Verify(
                s => s.CreateAsync(It.IsAny<string>(), It.IsAny<string?>(), It.IsAny<CancellationToken>()),
                Times.Never);
        }

        [TestMethod]
        public async Task ResolveOrCreateAsync_CuandoNoExiste_CreaYRetornaNuevoId()
        {
            _proyectoStrategy
                .Setup(s => s.FindExistingIdAsync("Proyecto Nuevo", null, It.IsAny<CancellationToken>()))
                .ReturnsAsync((string?)null);
            _proyectoStrategy
                .Setup(s => s.CreateAsync("Proyecto Nuevo", null, It.IsAny<CancellationToken>()))
                .ReturnsAsync("PROY-NUEVO");

            string result = await _repository.ResolveOrCreateAsync(
                HierarchyEntityKind.Proyecto,
                "Proyecto Nuevo",
                parentId: null,
                CancellationToken.None);

            Assert.AreEqual("PROY-NUEVO", result);
            _proyectoStrategy.Verify(
                s => s.CreateAsync("Proyecto Nuevo", null, It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [TestMethod]
        public void Constructor_SinStrategyRequerida_LanzaInvalidOperationException()
        {
            try
            {
                _ = new GetOrCreateRepository(
                    HierarchyIdentityPolicy.Instance,
                    new[] { _proyectoStrategy.Object });
                Assert.Fail("Se esperaba InvalidOperationException.");
            }
            catch (InvalidOperationException)
            {
            }
        }

        private static Mock<IHierarchyEntityStrategy> CreateStrategyMock(HierarchyEntityKind kind)
        {
            Mock<IHierarchyEntityStrategy> mock = new Mock<IHierarchyEntityStrategy>();
            mock.SetupGet(s => s.Kind).Returns(kind);
            return mock;
        }
    }
}
