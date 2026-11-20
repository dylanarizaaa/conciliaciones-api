using Conciliaciones.Dominio.DTOs.Response.Middlewares;
using Conciliaciones.Dominio.Ports.Interfaces.Middleware;
using Conciliaciones.Dominio.Utils;
using System.Text.Json;

namespace Conciliaciones.Api.Middlewares
{
    public class ErrorHandlerMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ErrorHandlerMiddleware> _logger;
        private readonly IExceptionMapper _exceptionMapper;

        private static readonly JsonSerializerOptions JsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        public ErrorHandlerMiddleware(RequestDelegate next, ILogger<ErrorHandlerMiddleware> logger, IExceptionMapper exceptionMapper)
        {
            _next = next;
            _logger = logger;
            _exceptionMapper = exceptionMapper;
        }

        public async Task Invoke(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception exception)
            {
                if (context.RequestAborted.IsCancellationRequested && exception is OperationCanceledException)
                {
                    return;
                }

                ExceptionResponse mapped = _exceptionMapper.Map(exception);
                await WriteResponseAsync(context, mapped, exception);
            }
        }

        private async Task WriteResponseAsync(HttpContext context, ExceptionResponse mapped, Exception ex)
        {
            _logger.LogError(
                ex,
                "Error en ErrorHandlerMiddleware a las {DateTime}, con código {StatusCode}",
                ConstantData.FechaActual,
                mapped.StatusCode);

            if (context.Response.HasStarted)
            {
                return;
            }

            context.Response.StatusCode = mapped.StatusCode;
            context.Response.ContentType = "application/json";
            string jsonResponse = JsonSerializer.Serialize(mapped.Response, JsonOptions);
            await context.Response.WriteAsync(jsonResponse, context.RequestAborted);
        }
    }
}
