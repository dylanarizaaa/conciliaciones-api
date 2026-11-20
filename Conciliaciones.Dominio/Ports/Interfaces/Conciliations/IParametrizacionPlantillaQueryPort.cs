namespace Conciliaciones.Dominio.Ports.Interfaces.Conciliations;

public interface IParametrizacionPlantillaQueryPort
{
    Task<string?> GetPlantillaJsonAsync(
        string reglaId,
        string tipoBlock,
        CancellationToken cancellationToken);
}
