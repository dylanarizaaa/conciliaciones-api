using Conciliaciones.Dominio.Ports.Interfaces.Hierarchy;

namespace Conciliaciones.Infraestructura.Persistence.Repositories
{
    /// <summary>
    /// Fachada tipada sobre <see cref="IGetOrCreatePort"/> (Repository Pattern + DIP).
    /// Preserva el contrato público de jerarquía sin duplicar lógica de creación.
    /// </summary>
    public class HierarchyWriteRepository : IHierarchyWritePort
    {
        private readonly IGetOrCreatePort _getOrCreatePort;

        public HierarchyWriteRepository(IGetOrCreatePort getOrCreatePort)
        {
            _getOrCreatePort = getOrCreatePort ?? throw new ArgumentNullException(nameof(getOrCreatePort));
        }

        public Task<string> ResolveOrCreateProyectoAsync(string proyecto, CancellationToken cancellationToken) =>
            _getOrCreatePort.ResolveOrCreateAsync(HierarchyEntityKind.Proyecto, proyecto, parentId: null, cancellationToken);

        public Task<string> ResolveOrCreateProcesoAsync(string proceso, string idProyecto, CancellationToken cancellationToken) =>
            _getOrCreatePort.ResolveOrCreateAsync(HierarchyEntityKind.Proceso, proceso, idProyecto, cancellationToken);

        public Task<string> ResolveOrCreateSubProcesoAsync(string subproceso, string idProceso, CancellationToken cancellationToken) =>
            _getOrCreatePort.ResolveOrCreateAsync(HierarchyEntityKind.SubProceso, subproceso, idProceso, cancellationToken);
    }
}
