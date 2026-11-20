namespace Conciliaciones.Infraestructura.Ports.Interfaces.AWS
{
    public interface IParameterStoreService
    {
        Task<string> GetValueParameterStore(string nombreParametro);

        Task<string> GetValueParameterStore(string nombreParametro, CancellationToken cancellationToken);
    }
}
