using Ardalis.GuardClauses;

namespace Conciliaciones.Dominio.Entities.Common
{
    public static class EntityValidation
    {
        public static string Required(string? value, string name)
        {
            Guard.Against.NullOrWhiteSpace(value, name);
            return value!.Trim();
        }

        public static string OptionalTrim(string? value) =>
            string.IsNullOrWhiteSpace(value) ? string.Empty : value.Trim();

        public static string NewId() => Guid.NewGuid().ToString();

        public static string ToIso(DateTime utc) => utc.ToUniversalTime().ToString("o");

        public static DateTime FromIsoOrUtcNow(string? iso)
        {
            if (!string.IsNullOrWhiteSpace(iso) && DateTime.TryParse(iso, out DateTime parsed))
            {
                return parsed.ToUniversalTime();
            }

            return DateTime.UtcNow;
        }
    }
}
