using Conciliaciones.Dominio.Ports.Interfaces.Hierarchy;

namespace Conciliaciones.Dominio.Services
{
    /// <summary>
    /// Política por defecto: un valor Guid se interpreta como identificador existente.
    /// </summary>
    public sealed class HierarchyIdentityPolicy : IIdentityResolutionPolicy
    {
        public static HierarchyIdentityPolicy Instance { get; } = new HierarchyIdentityPolicy();

        public bool IsExistingIdentifier(string? value) => Guid.TryParse(value, out _);

        /// <summary>Compatibilidad con llamadas estáticas existentes.</summary>
        public static bool UsesExistingIdentifier(string? value) => Instance.IsExistingIdentifier(value);
    }
}
