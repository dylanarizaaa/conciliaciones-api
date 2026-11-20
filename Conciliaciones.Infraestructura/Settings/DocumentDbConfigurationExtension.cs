using Microsoft.Extensions.DependencyInjection;
using System.Diagnostics.CodeAnalysis;

namespace Conciliaciones.Infraestructura.Settings
{
    [ExcludeFromCodeCoverage]
    public static class DocumentDbConfigurationExtension
    {
        [Obsolete("La conexión se inicializa de forma asíncrona en DocumentDbStartupHostedService.")]
        public static DocumentDbSettings AddDocumentDbConfiguration(this IServiceCollection services)
        {
            throw new InvalidOperationException("Use DocumentDbStartupHostedService para inicializar DocumentDB de forma asíncrona.");
        }
    }
}
