using Conciliaciones.Dominio.DTOs.Response.Middlewares;
using Conciliaciones.Dominio.Envoltorios;
using Conciliaciones.Dominio.Exceptions;
using Conciliaciones.Dominio.Ports.Interfaces.Middleware;
using System.Net;

namespace Conciliaciones.Infraestructura.Services
{
    public class ExceptionMapper : IExceptionMapper
    {
        public ExceptionResponse Map(Exception exception)
        {
            if (exception == null)
            {
                return CreateResponse(HttpStatusCode.InternalServerError, "Se ha producido un error interno no especificado.");
            }

            return exception switch
            {
                ApiException e => CreateResponse(HttpStatusCode.BadRequest, e.Message),
                KeyNotFoundException e => CreateResponse(HttpStatusCode.NotFound, e.Message),
                ValidationException e => CreateResponse(
                    HttpStatusCode.BadRequest,
                    "Se han encontrado errores de validación.",
                    e.Errors),
                UnauthorizedAccessException => CreateResponse(
                    HttpStatusCode.Unauthorized,
                    "Acceso no autorizado."),
                HttpRequestException => CreateResponse(
                    HttpStatusCode.ServiceUnavailable,
                    "Error de conexión con servicio externo"),
                OperationCanceledException => CreateResponse(
                    HttpStatusCode.RequestTimeout,
                    "Tiempo de espera agotado"),
                _ => CreateResponse(
                    HttpStatusCode.InternalServerError,
                    IsDevelopment()
                        ? exception.Message
                        : "Se ha producido un error interno.")
            };
        }

        private static bool IsDevelopment() =>
            string.Equals(
                Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT"),
                "Development",
                StringComparison.OrdinalIgnoreCase);

        private static ExceptionResponse CreateResponse(
            HttpStatusCode statusCode,
            string message,
            IEnumerable<string>? errores = null)
        {
            return new ExceptionResponse
            {
                StatusCode = (int)statusCode,
                Response = new Answers<string>
                {
                    RespuestaEstado = false,
                    Mensaje = message,
                    CodigoEstado = statusCode,
                    Errores = errores?.ToList() ?? new List<string>()
                }
            };
        }
    }
}
