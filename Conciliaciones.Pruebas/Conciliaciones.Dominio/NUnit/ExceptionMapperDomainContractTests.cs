using Conciliaciones.Infraestructura.Services;
using FluentAssertions;
using NUnit.Framework;
using System.Net;
using ApiException = Conciliaciones.Dominio.Exceptions.ApiException;
using DomainValidationException = Conciliaciones.Dominio.Exceptions.ValidationException;

namespace Conciliaciones.Pruebas.Conciliaciones.Dominio.NUnit
{
    [TestFixture]
    public class ExceptionMapperDomainContractTests
    {
        [Test]
        public void Map_DomainValidationException_ReturnsBadRequestWithErrors()
        {
            // Arrange
            ExceptionMapper mapper = new();
            DomainValidationException ex = new(new[]
            {
                new FluentValidation.Results.ValidationFailure("Field", "msg")
            });

            // Act
            var response = mapper.Map(ex);

            // Assert
            response.StatusCode.Should().Be((int)HttpStatusCode.BadRequest);
            response.Response.Errores.Should().Contain("msg");
            response.Response.Mensaje.Should().Contain("validación");
        }

        [Test]
        public void Map_ApiException_ReturnsBadRequest()
        {
            ExceptionMapper mapper = new();
            var response = mapper.Map(new ApiException("bad"));
            response.StatusCode.Should().Be((int)HttpStatusCode.BadRequest);
            response.Response.Mensaje.Should().Be("bad");
        }

        [Test]
        public void Map_Null_ReturnsInternalServerError()
        {
            ExceptionMapper mapper = new();
            var response = mapper.Map(null!);
            response.StatusCode.Should().Be((int)HttpStatusCode.InternalServerError);
            response.Response.Mensaje.Should().Contain("error interno");
        }

        [Test]
        public void Map_UnhandledException_DoesNotLeakMessage_OutsideDevelopment()
        {
            string? previous = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
            try
            {
                Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", "Production");
                ExceptionMapper mapper = new();
                var response = mapper.Map(new InvalidOperationException("mongodb://secret-host:27017"));
                response.StatusCode.Should().Be((int)HttpStatusCode.InternalServerError);
                response.Response.Mensaje.Should().Be("Se ha producido un error interno.");
                response.Response.Mensaje.Should().NotContain("secret-host");
            }
            finally
            {
                Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", previous);
            }
        }

        [Test]
        public void Map_UnhandledException_InDevelopment_ExposesMessageForDiagnostics()
        {
            string? previous = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
            try
            {
                Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", "Development");
                ExceptionMapper mapper = new();
                var response = mapper.Map(new InvalidOperationException("timeout DocumentDB"));
                response.Response.Mensaje.Should().Be("timeout DocumentDB");
            }
            finally
            {
                Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", previous);
            }
        }
    }
}
