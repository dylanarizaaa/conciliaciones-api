namespace Conciliaciones.Dominio.Ports.Interfaces.Hierarchy
{
    public interface IIdentityResolutionPolicy
    {
        bool IsExistingIdentifier(string? value);
    }
}
