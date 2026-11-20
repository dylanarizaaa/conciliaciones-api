using System.Net;

namespace Conciliaciones.Dominio.Envoltorios
{
    public class Answers<T>
    {
        public HttpStatusCode CodigoEstado { get; set; }
        public bool RespuestaEstado { get; set; }
        public string? Mensaje { get; set; }
        public List<string> Errores { get; set; } = new List<string>();
        public string? MensajeError { get; set; }
        public T? Resultado { get; set; }
        public Answers()
        {
            CodigoEstado = HttpStatusCode.OK;
            RespuestaEstado = true;
        }
        public Answers(T datos, string? mensaje = null, HttpStatusCode codigo = HttpStatusCode.OK)

        {
            CodigoEstado = codigo;
            RespuestaEstado = true;
            Mensaje = mensaje;
            Resultado = datos;

        }
        public Answers(string mensaje, HttpStatusCode codigo = HttpStatusCode.NotFound)
        {
            CodigoEstado = codigo;
            RespuestaEstado = false;
            Mensaje = mensaje;
        }

    }
}
