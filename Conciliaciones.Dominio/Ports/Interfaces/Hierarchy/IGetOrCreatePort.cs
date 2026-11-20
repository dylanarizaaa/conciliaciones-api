namespace Conciliaciones.Dominio.Ports.Interfaces.Hierarchy
{
    /// <summary>
    /// Puerto genérico Get-Or-Create (DIP). La resolución de identidad y la persistencia
    /// se delegan a políticas/estrategias en infraestructura.
    /// </summary>
    public interface IGetOrCreatePort
    {
        Task<string> ResolveOrCreateAsync(
            HierarchyEntityKind kind,
            string candidate,
            string? parentId,
            CancellationToken cancellationToken);
    }
}
