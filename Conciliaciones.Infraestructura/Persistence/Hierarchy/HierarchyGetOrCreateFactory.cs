using Conciliaciones.Dominio.Ports.Interfaces.Hierarchy;
using Conciliaciones.Dominio.Services;
using Conciliaciones.Infraestructura.Persistence.Hierarchy.Strategies;
using Conciliaciones.Infraestructura.Persistence.Repositories;
using Conciliaciones.Infraestructura.Ports.Interfaces.DBContext;
using Conciliaciones.Infraestructura.Ports.Interfaces.Persistence;

namespace Conciliaciones.Infraestructura.Persistence.Hierarchy
{
    /// <summary>
    /// Composición manual para escenarios sin DI (p.ej. constructores de compatibilidad).
    /// </summary>
    public static class HierarchyGetOrCreateFactory
    {
        public static IGetOrCreatePort CreateGetOrCreatePort(IDocumentDBContext dbContext)
        {
            IHierarchyEntityStrategy[] strategies =
            {
                new ProyectoHierarchyStrategy(dbContext),
                new ProcesoHierarchyStrategy(dbContext),
                new SubProcesoHierarchyStrategy(dbContext)
            };

            return new GetOrCreateRepository(HierarchyIdentityPolicy.Instance, strategies);
        }

        public static IHierarchyWritePort CreateHierarchyWritePort(IDocumentDBContext dbContext) =>
            new HierarchyWriteRepository(CreateGetOrCreatePort(dbContext));
    }
}
