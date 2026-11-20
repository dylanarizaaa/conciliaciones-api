using Conciliaciones.Dominio.Exceptions;
using FluentValidation.Results;

namespace Conciliaciones.Pruebas.Conciliaciones.Dominio.Exceptions
{
    [TestClass]
    public class ValidationExceptionTests
    {
        [TestMethod]
        public void Constructor_SinParametros_InicializaMensajePorDefectoYListaVacia()
        {
            // Arrange & Act
            ValidationException exception = new ValidationException();

            // Assert
            Assert.IsNotNull(exception);
            Assert.AreEqual("Se han producido uno o más errores de validación.", exception.Message);
            Assert.IsNotNull(exception.Errors);
            Assert.IsEmpty(exception.Errors);
        }

        [TestMethod]
        public void Constructor_ConColeccionDeFallos_MapeaMensajesDeErrorCorrectamente()
        {
            // Arrange
            List<ValidationFailure> failures = new List<ValidationFailure>
            {
                new ValidationFailure("Monto", "El monto afectado debe ser mayor a cero."),
                new ValidationFailure("SubprocesoId", "El ID del subproceso no es válido.")
            };

            // Act
            ValidationException exception = new ValidationException(failures);

            // Assert
            Assert.IsNotNull(exception);
            Assert.IsNotNull(exception.Errors);
            Assert.HasCount(2, exception.Errors);
            Assert.AreEqual("El monto afectado debe ser mayor a cero.", exception.Errors[0]);
            Assert.AreEqual("El ID del subproceso no es válido.", exception.Errors[1]);
        }

        [TestMethod]
        public void Constructor_ConColeccionNula_NoLanzaExcepcionYMantieneListaVacia()
        {
            // Arrange & Act
            ValidationException exception = new ValidationException(null!);

            // Assert
            Assert.IsNotNull(exception);
            Assert.IsNotNull(exception.Errors);
            Assert.IsEmpty(exception.Errors);
        }
    }
}