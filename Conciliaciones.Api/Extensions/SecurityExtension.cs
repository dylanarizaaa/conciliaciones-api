using IntegracionSeguridadKeycloak.Core;
using IntegracionSeguridadKeycloak.Core.Shared.Configuraciones;
using IntegracionSeguridadKeycloak.Core.Shared.Seguridad;
using System.Diagnostics.CodeAnalysis;

namespace Conciliaciones.Api.Extensions
{
    [ExcludeFromCodeCoverage]
    public static class SecurityExtension
    {
        public static IServiceCollection AddKeycloakSecurityConfiguration(this IServiceCollection services, AppConfiguracion appConfiguracion)
        {
            if (appConfiguracion == null)
            {
                throw new ArgumentNullException(nameof(appConfiguracion), "La configuración de la aplicación no puede ser nula.");
            }

            string authority = appConfiguracion.KeycloakAuthority ?? string.Empty;
            string audience = appConfiguracion.KeycloakAudience ?? string.Empty;
            bool requireHttps = appConfiguracion.KeycloakRequireHttps;
            bool validateAudience = string.Equals(audience.ToLower(), "true", StringComparison.OrdinalIgnoreCase);

            string pdb = appConfiguracion.PDB ?? throw new InvalidOperationException("El parámetro PDB no está configurado.");
            string llaveCifrada = appConfiguracion.LLAVE_CIFRADA ?? throw new InvalidOperationException("El parámetro LLAVE_CIFRADA no está configurado.");
            string requiredRolesRaw = appConfiguracion.RequiredRoles ?? string.Empty;

            string rolesDescifrados = Encripcion.Descifrar(requiredRolesRaw, pdb, llaveCifrada);

            KeycloakSettings keycloakSettings = new KeycloakSettings
            {
                Authority = authority,
                Audience = audience,
                RequireHttps = requireHttps,
                ValidateAudience = validateAudience,
                RequiredRoles = rolesDescifrados
            };

            services.AddKeycloakSecurity(keycloakSettings);
            return services;
        }
    }
}