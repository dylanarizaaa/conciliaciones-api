using Conciliaciones.Aplicacion.Common.Behaviors;
using Conciliaciones.Aplicacion.Ports.Interfaces;
using Conciliaciones.Aplicacion.Services;
using Conciliaciones.Dominio.Services;
using FluentValidation;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;


namespace Conciliaciones.Aplicacion
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddApplicationLayer(this IServiceCollection services, IConfiguration configuration)
        {
            if (services == null)
            {
                throw new ArgumentNullException(nameof(services), "El contenedor de servicios no puede ser nulo.");
            }
            Assembly currentAssembly = Assembly.GetExecutingAssembly();
            services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());
            services.AddSingleton<ITransformacionMontoCalculator, TransformacionMontoCalculator>();
            services.AddSingleton<ConciliationUiComposer>();
            services.AddScoped<IConciliationQueryService, ConciliationQueryService>();
            services.AddScoped<IConciliationStateService, ConciliationStateService>();
            services.AddScoped<IParametrizacionApplicationService, ParametrizacionApplicationService>();
            services.AddMediatR(cfg =>
            {
                cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly());
                cfg.AddOpenBehavior(typeof(ValidationBehavior<,>));
            });
            return services;
        }
    }
}
