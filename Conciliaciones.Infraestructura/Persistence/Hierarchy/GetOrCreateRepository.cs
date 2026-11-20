using Conciliaciones.Dominio.Ports.Interfaces.Hierarchy;
using Conciliaciones.Infraestructura.Ports.Interfaces.Persistence;

namespace Conciliaciones.Infraestructura.Persistence.Hierarchy
{
    /// <summary>
    /// Repositorio genérico Get-Or-Create. Orquesta política de identidad + strategies.
    /// </summary>
    public sealed class GetOrCreateRepository : IGetOrCreatePort
    {
        private readonly IIdentityResolutionPolicy _identityPolicy;
        private readonly IReadOnlyDictionary<HierarchyEntityKind, IHierarchyEntityStrategy> _strategies;

        public GetOrCreateRepository(
            IIdentityResolutionPolicy identityPolicy,
            IEnumerable<IHierarchyEntityStrategy> strategies)
        {
            _identityPolicy = identityPolicy ?? throw new ArgumentNullException(nameof(identityPolicy));
            if (strategies == null)
            {
                throw new ArgumentNullException(nameof(strategies));
            }

            _strategies = strategies.ToDictionary(s => s.Kind);
            EnsureRequiredStrategies();
        }

        public async Task<string> ResolveOrCreateAsync(
            HierarchyEntityKind kind,
            string candidate,
            string? parentId,
            CancellationToken cancellationToken)
        {
            IHierarchyEntityStrategy strategy = ResolveStrategy(kind);

            if (_identityPolicy.IsExistingIdentifier(candidate))
            {
                if (!await strategy.ExistsByIdAsync(candidate, cancellationToken))
                {
                    throw new KeyNotFoundException($"No existe {kind} con identificador '{candidate}'.");
                }

                return candidate;
            }

            string? existingId = await strategy.FindExistingIdAsync(candidate, parentId, cancellationToken);
            if (existingId != null)
            {
                return existingId;
            }

            return await strategy.CreateAsync(candidate, parentId, cancellationToken);
        }

        private IHierarchyEntityStrategy ResolveStrategy(HierarchyEntityKind kind) =>
            _strategies[kind];

        private void EnsureRequiredStrategies()
        {
            foreach (HierarchyEntityKind kind in Enum.GetValues<HierarchyEntityKind>())
            {
                if (!_strategies.ContainsKey(kind))
                {
                    throw new InvalidOperationException($"Falta registrar IHierarchyEntityStrategy para '{kind}'.");
                }
            }
        }
    }
}
