using Conciliaciones.Aplicacion.Common.Behaviors;
using FluentValidation;
using FluentValidation.Results;
using MediatR;
using Moq;
using DomainValidationException = Conciliaciones.Dominio.Exceptions.ValidationException;

namespace Conciliaciones.Pruebas.Conciliaciones.Aplicacion.Common.Behaviors
{
    // Clases DTO de soporte para las pruebas de MediatR
    public class DummyRequest : IRequest<DummyResponse>
    {
        public string Nombre { get; set; } = string.Empty;
    }

    public class DummyResponse
    {
        public bool Exitoso { get; set; }
    }

    [TestClass]
    public class ValidationBehaviorTests
    {
        [TestMethod]
        public void Constructor_ConValidadoresNulos_LanzaArgumentNullException()
        {
            // Arrange & Act & Assert
            Assert.ThrowsExactly<ArgumentNullException>(() =>
                new ValidationBehavior<DummyRequest, DummyResponse>(null!));
        }

        [TestMethod]
        public async Task Handle_SinValidadores_EjecutaSiguienteDelegateExitosamente()
        {
            // Arrange
            List<IValidator<DummyRequest>> validators = new List<IValidator<DummyRequest>>();
            ValidationBehavior<DummyRequest, DummyResponse> behavior = new ValidationBehavior<DummyRequest, DummyResponse>(validators);

            DummyRequest request = new DummyRequest { Nombre = "Prueba" };
            DummyResponse responseEsperada = new DummyResponse { Exitoso = true };

            RequestHandlerDelegate<DummyResponse> nextDelegate = (cancellationToken) =>
            {
                return Task.FromResult(new DummyResponse
                {
                    Exitoso = true
                });
            };

            // Act
            DummyResponse resultado = await behavior.Handle(request, nextDelegate, CancellationToken.None);

            // Assert
            Assert.IsNotNull(resultado);
            Assert.AreEqual(responseEsperada.Exitoso, resultado.Exitoso);
        }

        [TestMethod]
        public async Task Handle_ConValidacionesCorrectas_EjecutaSiguienteDelegateExitosamente()
        {
            // Arrange
            Mock<IValidator<DummyRequest>> validatorMock = new Mock<IValidator<DummyRequest>>();
            ValidationResult validationResult = new ValidationResult();

            validatorMock
                .Setup(v => v.ValidateAsync(It.IsAny<ValidationContext<DummyRequest>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(validationResult);

            List<IValidator<DummyRequest>> validators = new List<IValidator<DummyRequest>> { validatorMock.Object };
            ValidationBehavior<DummyRequest, DummyResponse> behavior = new ValidationBehavior<DummyRequest, DummyResponse>(validators);

            DummyRequest request = new DummyRequest { Nombre = "PruebaValida" };
            DummyResponse responseEsperada = new DummyResponse { Exitoso = true };

            RequestHandlerDelegate<DummyResponse> nextDelegate = (cancellationToken) =>
            {
                return Task.FromResult(new DummyResponse
                {
                    Exitoso = true
                });
            };

            // Act
            DummyResponse resultado = await behavior.Handle(request, nextDelegate, CancellationToken.None);

            // Assert
            Assert.IsNotNull(resultado);
            Assert.AreEqual(responseEsperada.Exitoso, resultado.Exitoso);
            validatorMock.Verify(v => v.ValidateAsync(It.IsAny<ValidationContext<DummyRequest>>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        [TestMethod]
        public async Task Handle_ConErroresDeValidacion_LanzaValidationException()
        {
            // Arrange
            Mock<IValidator<DummyRequest>> validatorMock = new Mock<IValidator<DummyRequest>>();
            List<ValidationFailure> failures = new List<ValidationFailure>
            {
                new ValidationFailure("Nombre", "El campo Nombre no puede estar vacío.")
            };
            ValidationResult validationResult = new ValidationResult(failures);

            validatorMock
                .Setup(v => v.ValidateAsync(It.IsAny<ValidationContext<DummyRequest>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(validationResult);

            List<IValidator<DummyRequest>> validators = new List<IValidator<DummyRequest>> { validatorMock.Object };
            ValidationBehavior<DummyRequest, DummyResponse> behavior = new ValidationBehavior<DummyRequest, DummyResponse>(validators);

            DummyRequest request = new DummyRequest { Nombre = "" };
            RequestHandlerDelegate<DummyResponse> nextDelegate = (cancellationToken) =>
         {
             return Task.FromResult(new DummyResponse
             {
                 Exitoso = true
             });
         };

            // Act & Assert
            DomainValidationException exception = await Assert.ThrowsExactlyAsync<DomainValidationException>(() =>
                behavior.Handle(request, nextDelegate, CancellationToken.None));

            Assert.IsNotNull(exception.Errors);
            Assert.AreEqual(1, exception.Errors.Count);
        }
    }
}