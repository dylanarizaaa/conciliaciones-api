namespace Conciliaciones.Dominio.Ports.Interfaces.Hierarchy
{
    public interface IHierarchyWritePort
    {
        Task<string> ResolveOrCreateProyectoAsync(string proyecto, CancellationToken cancellationToken);

        Task<string> ResolveOrCreateProcesoAsync(string proceso, string idProyecto, CancellationToken cancellationToken);

        Task<string> ResolveOrCreateSubProcesoAsync(string subproceso, string idProceso, CancellationToken cancellationToken);
    }
}
