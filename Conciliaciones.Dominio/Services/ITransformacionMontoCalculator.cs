namespace Conciliaciones.Dominio.Services
{
    public interface ITransformacionMontoCalculator
    {
        decimal Sum(IEnumerable<decimal> valores);

        decimal ParseAmount(object? raw);
    }
}
