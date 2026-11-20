using Conciliaciones.Api.Extensions;
using Conciliaciones.Api.Middlewares;
using Conciliaciones.Aplicacion;
using Conciliaciones.Infraestructura;
using DotNetEnv;
using IntegracionSeguridadKeycloak.Core.Shared.Configuraciones;
using System.Diagnostics.CodeAnalysis;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

Env.Load();

AppConfiguracion appConfiguracion = builder.Services.AddAppConfiguration();

builder.Services.AddHttpContextAccessor();
builder.Services.AddSingleton(appConfiguracion);
builder.Services.AddKeycloakSecurityConfiguration(appConfiguracion);
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddApplicationLayer(builder.Configuration);
builder.Services.AddInfrastructureLayer(builder.Configuration);
builder.Services.AddKeyedSingleton<AppConfiguracion>(appConfiguracion);
WebApplication app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Error handler first so auth/controller failures map to Answers consistently.
app.UseMiddleware<ErrorHandlerMiddleware>();
app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

await app.RunAsync();

[ExcludeFromCodeCoverage]
public partial class Program
{
    protected Program()
    {
    }
}
