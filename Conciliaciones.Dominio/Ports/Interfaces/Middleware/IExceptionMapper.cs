using Conciliaciones.Dominio.DTOs.Response.Middlewares;

namespace Conciliaciones.Dominio.Ports.Interfaces.Middleware
{
    public interface IExceptionMapper
    {
        ExceptionResponse Map(Exception exception);
    }
}
