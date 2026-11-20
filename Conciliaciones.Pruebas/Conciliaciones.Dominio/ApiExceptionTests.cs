using Conciliaciones.Dominio.Exceptions;

namespace Conciliaciones.Pruebas.Conciliaciones.Dominio.Exceptions
{
    [TestClass]
    public class ApiExceptionTests
    {
        [TestMethod]
        public void Constructor_SinParametros_CreaInstanciaCorrectamente()
        {
            // Arrange & Act
            ApiException exception = new ApiException();

            // Assert
            Assert.IsNotNull(exception);
            Assert.IsInstanceOfType(exception, typeof(Exception));
        }

        [TestMethod]
        public void Constructor_ConMensajeSimple_AsignaMensajeExitosamente()
        {
            // Arrange
            string mensajeEsperado = "Ocurrió un error en la capa de dominio.";

            // Act
            ApiException exception = new ApiException(mensajeEsperado);

            // Assert
            Assert.IsNotNull(exception);
            Assert.AreEqual(mensajeEsperado, exception.Message);
        }

        [TestMethod]
        public void Constructor_ConMensajeFormateadoYArgumentos_FormateaMensajeCorrectamente()
        {
            // Arrange
            string plantillaMensaje = "Error al procesar la conciliación con ID {0} para el proyecto {1}.";
            string idConciliacion = "CONC-12345";
            string idProyecto = "PROY-999";
            string mensajeEsperado = "Error al procesar la conciliación con ID CONC-12345 para el proyecto PROY-999.";

            // Act
            ApiException exception = new ApiException(plantillaMensaje, idConciliacion, idProyecto);

            // Assert
            Assert.IsNotNull(exception);
            Assert.AreEqual(mensajeEsperado, exception.Message);
        }
    }
}