using Conciliaciones.Dominio.Envoltorios;
using System.Net;

namespace Conciliaciones.Pruebas.Conciliaciones.Dominio.Envoltorios
{
    [TestClass]
    public class AnswersTests
    {
        [TestMethod]
        public void Constructor_SinParametros_InicializaConValoresPorDefectoExitosos()
        {
            // Arrange & Act
            Answers<string> respuesta = new Answers<string>();

            // Assert
            Assert.IsNotNull(respuesta);
            Assert.AreEqual(HttpStatusCode.OK, respuesta.CodigoEstado);
            Assert.IsTrue(respuesta.RespuestaEstado);
            Assert.IsNull(respuesta.Mensaje);
            Assert.IsNull(respuesta.Resultado);
            Assert.IsNotNull(respuesta.Errores);
            Assert.AreEqual(0, respuesta.Errores.Count);
        }

        [TestMethod]
        public void Constructor_ConResultadoMensajeYCodigo_InicializaRespuestaExitosa()
        {
            // Arrange
            string ResultadoMock = "Resultado de prueba de conciliación";
            string mensajeMock = "Proceso completado exitosamente";
            HttpStatusCode codigoMock = HttpStatusCode.Created;

            // Act
            Answers<string> respuesta = new Answers<string>(ResultadoMock, mensajeMock, codigoMock);

            // Assert
            Assert.IsNotNull(respuesta);
            Assert.IsTrue(respuesta.RespuestaEstado);
            Assert.AreEqual(codigoMock, respuesta.CodigoEstado);
            Assert.AreEqual(mensajeMock, respuesta.Mensaje);
            Assert.AreEqual(ResultadoMock, respuesta.Resultado);
        }

        [TestMethod]
        public void Constructor_ConMensajeYCodigoError_InicializaRespuestaFallida()
        {
            // Arrange
            string mensajeErrorMock = "Recurso no encontrado";
            HttpStatusCode codigoErrorMock = HttpStatusCode.NotFound;

            // Act
            Answers<string> respuesta = new Answers<string>(mensajeErrorMock, codigoErrorMock);

            // Assert
            Assert.IsNotNull(respuesta);
            Assert.IsFalse(respuesta.RespuestaEstado);
            Assert.AreEqual(codigoErrorMock, respuesta.CodigoEstado);
            Assert.AreEqual(mensajeErrorMock, respuesta.Mensaje);
            Assert.IsNull(respuesta.Resultado);
        }

        [TestMethod]
        public void Propiedades_AsignacionManual_ActualizaValoresCorrectamente()
        {
            // Arrange
            Answers<List<int>> respuesta = new Answers<List<int>>();
            List<int> listaResultado = new List<int> { 1, 2, 3 };
            List<string> listaErrores = new List<string> { "Error de prueba" };

            // Act
            respuesta.Resultado = listaResultado;
            respuesta.Errores = listaErrores;
            respuesta.MensajeError = "Ocurrió una falla interna";

            // Assert
            Assert.IsNotNull(respuesta.Resultado);
            Assert.AreEqual(3, respuesta.Resultado.Count);
            Assert.AreEqual(1, respuesta.Errores.Count);
            Assert.AreEqual("Ocurrió una falla interna", respuesta.MensajeError);
        }
    }
}