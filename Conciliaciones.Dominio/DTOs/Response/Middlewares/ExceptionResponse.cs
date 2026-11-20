using Conciliaciones.Dominio.Envoltorios;

namespace Conciliaciones.Dominio.DTOs.Response.Middlewares
{
    public sealed class ExceptionResponse
    {
        public int StatusCode { get; set; }

        public Answers<string> Response { get; set; } = default!;
    }
}
