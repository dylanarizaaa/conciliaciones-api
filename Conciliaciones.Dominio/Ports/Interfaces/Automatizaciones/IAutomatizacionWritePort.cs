using Conciliaciones.Dominio.DTOs.Requests.Automatizaciones;
using Conciliaciones.Dominio.DTOs.Response.Automatizaciones;
using System.Net;

namespace Conciliaciones.Dominio.Ports.Interfaces.Automatizaciones;

public interface IAutomatizacionWritePort
{
    Task<(HttpStatusCode Status, CreateAutomatizacionResponse? Data, string Mensaje)> CreateAsync(
        CreateAutomatizacionRequest request,
        CancellationToken cancellationToken);
}
