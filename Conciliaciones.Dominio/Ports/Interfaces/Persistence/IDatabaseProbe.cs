namespace Conciliaciones.Dominio.Ports.Interfaces.Persistence
{
    public sealed record DatabasePingResult(
        bool Ok,
        string Status,
        string PingResult,
        string DatabaseName,
        IReadOnlyList<string> Collections,
        string? ErrorMessage,
        string? InnerException);

    public sealed record DatabaseCatalogResult(
        bool Ok,
        string Status,
        IReadOnlyList<string> Databases,
        string CurrentDatabase,
        string? ErrorMessage,
        string? InnerException);

    public interface IDatabaseProbe
    {
        Task<DatabasePingResult> PingAsync(CancellationToken cancellationToken);

        Task<DatabaseCatalogResult> ListDatabasesAsync(CancellationToken cancellationToken);
    }
}
