using Amazon;
using Amazon.AppConfigData;
using Amazon.SimpleSystemsManagement;
using Conciliaciones.Dominio.Ports.Interfaces.Automatizaciones;
using Conciliaciones.Dominio.Ports.Interfaces.Conciliations;
using Conciliaciones.Dominio.Ports.Interfaces.ExternalAPIs;
using Conciliaciones.Dominio.Ports.Interfaces.Hierarchy;
using Conciliaciones.Dominio.Ports.Interfaces.Middleware;
using Conciliaciones.Dominio.Ports.Interfaces.Persistence;
using Conciliaciones.Dominio.Ports.Interfaces.Process;
using Conciliaciones.Dominio.Ports.Interfaces.Proyects;
using Conciliaciones.Dominio.Ports.Interfaces.Reports;
using Conciliaciones.Dominio.Ports.Interfaces.SubProcess;
using Conciliaciones.Dominio.Services;
using Conciliaciones.Dominio.Settings;
using Conciliaciones.Infraestructura.Adapters;
using Conciliaciones.Infraestructura.Persistence.DocumentDB;
using Conciliaciones.Infraestructura.Persistence.Hierarchy;
using Conciliaciones.Infraestructura.Persistence.Hierarchy.Strategies;
using Conciliaciones.Infraestructura.Persistence.Repositories;
using Conciliaciones.Infraestructura.Persistence.Services;
using Conciliaciones.Infraestructura.Persistence.Services.Ui;
using Conciliaciones.Infraestructura.Ports.Interfaces.AWS;
using Conciliaciones.Infraestructura.Ports.Interfaces.DBContext;
using Conciliaciones.Infraestructura.Ports.Interfaces.Persistence;
using Conciliaciones.Infraestructura.Reports;
using Conciliaciones.Infraestructura.Services;
using Conciliaciones.Infraestructura.Settings.AmazonWebServices;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Diagnostics.CodeAnalysis;

namespace Conciliaciones.Infraestructura
{
    [ExcludeFromCodeCoverage]
    public static class DependencyInjection
    {
        public static void AddInfrastructureLayer(this IServiceCollection services, IConfiguration configuration)
        {
            if (services == null)
            {
                throw new ArgumentNullException(nameof(services), "El contenedor IServiceCollection no puede ser nulo.");
            }

            RegisterAwsClients(services);
            RegisterPersistence(services);
            RegisterCompositionServices(services);
            services.AddSingleton<IExceptionMapper, ExceptionMapper>();
        }

        private static void RegisterAwsClients(IServiceCollection services)
        {
            string regionNombre = Environment.GetEnvironmentVariable("Region") ?? "us-east-1";
            RegionEndpoint region = RegionEndpoint.GetBySystemName(new AwsSettings { Region = regionNombre }.Region);
            AmazonSimpleSystemsManagementClient parameterClient = new AmazonSimpleSystemsManagementClient(region);
            AmazonAppConfigDataClient appConfigClient = new AmazonAppConfigDataClient(region);

            services.AddMemoryCache();
            services.AddSingleton<IAmazonSimpleSystemsManagement>(parameterClient);
            services.AddSingleton<IAmazonAppConfigData>(appConfigClient);
            services.AddSingleton<IParameterStoreService, ParameterStorageService>();
        }

        private static void RegisterPersistence(IServiceCollection services)
        {
            services.AddSingleton<IDocumentDBContext>(sp => new DocumentDBContext(sp.GetRequiredService<IParameterStoreService>()));
            services.AddSingleton<IDocumentDbIndexInitializer, DocumentDbIndexInitializer>();
            services.AddHostedService<DocumentDbStartupHostedService>();
            services.AddScoped<IDatabaseProbe, DocumentDbProbeAdapter>();
            services.AddHealthChecks()
                .AddCheck<DocumentDbReadyHealthCheck>("documentdb", tags: new[] { "ready" });
            RegisterHierarchyGetOrCreate(services);
            services.AddScoped<IProyectsReportRepository, ProyectsReportRepository>();
            services.AddScoped<IProcessRepository, ProcessRepository>();
            services.AddScoped<ISubProcessRepository, SubProcessRepository>();
            services.AddScoped<IConciliationQueryPort, ConciliationQueryRepository>();
            services.AddScoped<IConciliationExportQueryPort, ConciliationExportRepository>();
            services.AddScoped<IConciliationStatePort, ConciliationStateRepository>();
            services.AddScoped<IParametrizacionWritePort, ParametrizacionWriteRepository>();
            services.AddScoped<IParametrizacionPlantillaQueryPort, ParametrizacionPlantillaRepository>();
            services.AddScoped<IExternalAPIsRepository, ExternalApisParametrizacion>();
            services.AddScoped<IAutomatizacionWritePort, AutomatizacionWriteRepository>();
            services.AddScoped<IConciliationsRepository, ConciliationsRepository>();
            services.AddSingleton<IConciliacionExcelReportPort, ConciliacionExcelReportService>();

        }

        private static void RegisterHierarchyGetOrCreate(IServiceCollection services)
        {
            services.AddSingleton<IIdentityResolutionPolicy, HierarchyIdentityPolicy>();
            services.AddScoped<IHierarchyEntityStrategy, ProyectoHierarchyStrategy>();
            services.AddScoped<IHierarchyEntityStrategy, ProcesoHierarchyStrategy>();
            services.AddScoped<IHierarchyEntityStrategy, SubProcesoHierarchyStrategy>();
            services.AddScoped<IGetOrCreatePort, GetOrCreateRepository>();
            services.AddScoped<IHierarchyWritePort, HierarchyWriteRepository>();
        }

        private static void RegisterCompositionServices(IServiceCollection services)
        {
            services.AddSingleton<IUiConfigColumnExtractor, UiConfigColumnExtractor>();
            services.AddSingleton<IConciliationRowsComposer, ConciliationRowsComposer>();
            services.AddSingleton<IConciliationExportComposer, ConciliationExportComposer>();
            services.AddSingleton<IConciliationFuentesDetailComposer, ConciliationFuentesDetailComposer>();
            services.AddSingleton<IFailedConciliationSummaryMapper, FailedConciliationSummaryMapper>();
            services.AddSingleton<IUiCellConfigStrategy, CurrencyUiCellConfigStrategy>();
            services.AddSingleton<IUiCellConfigStrategy, DateUiCellConfigStrategy>();
            services.AddSingleton<UiCellConfigFactory>();
        }
    }
}
