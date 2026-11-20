using Conciliaciones.Dominio.Ports.Interfaces.Hierarchy;

namespace Conciliaciones.Infraestructura.Ports.Interfaces.Persistence
{
    /// <summary>
    /// Strategy Pattern: cada entidad jerárquica encapsula lookup + creación.
    /// </summary>
    public interface IHierarchyEntityStrategy
    {
        HierarchyEntityKind Kind { get; }

        Task<string?> FindExistingIdAsync(string name, string? parentId, CancellationToken cancellationToken);

        Task<bool> ExistsByIdAsync(string id, CancellationToken cancellationToken);

        Task<string> CreateAsync(string name, string? parentId, CancellationToken cancellationToken);
    }
}
