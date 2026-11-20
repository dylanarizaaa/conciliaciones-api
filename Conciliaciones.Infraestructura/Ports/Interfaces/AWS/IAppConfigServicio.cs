namespace Conciliaciones.Infraestructura.Ports.Interfaces.AWS
{
    public interface IAppConfigServicio
    {
        Task<string> ObtenerConfiguracionAppConfig();

    }
}
