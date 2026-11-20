using Conciliaciones.Api.Middlewares;
using Conciliaciones.Dominio.DTOs.Response.Middlewares;
using Conciliaciones.Dominio.Envoltorios;
using Conciliaciones.Dominio.Exceptions;
using Conciliaciones.Dominio.Ports.Interfaces.Middleware;
using Conciliaciones.Infraestructura.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Moq;
using System.Net;

namespace Conciliaciones.Pruebas.Conciliaciones.Api.Middlewares
{
    [TestClass]
    public class ErrorHandlerMiddlewareTests
    {
        private Mock<ILogger<ErrorHandlerMiddleware>> _loggerMock = null!;
        private IExceptionMapper _exceptionMapper = null!;

        [TestInitialize]
        public void Setup()
        {
            _loggerMock = new Mock<ILogger<ErrorHandlerMiddleware>>();
            _exceptionMapper = new ExceptionMapper();
        }

        [TestMethod]
        public async Task Invoke_SinExcepcion_InvocaSiguienteDelegateExitosamente()
        {
            bool delegateInvocado = false;
            RequestDelegate nextDelegate = _ =>
            {
                delegateInvocado = true;
                return Task.CompletedTask;
            };

            DefaultHttpContext httpContext = new DefaultHttpContext();
            ErrorHandlerMiddleware middleware = CreateMiddleware(nextDelegate);

            await middleware.Invoke(httpContext);

            Assert.IsTrue(delegateInvocado);
            Assert.AreEqual((int)HttpStatusCode.OK, httpContext.Response.StatusCode);
        }

        [TestMethod]
        public async Task Invoke_ApiExceptionLanzada_RetornaStatusCodeBadRequest()
        {
            RequestDelegate nextDelegate = _ => throw new ApiException("Error de Api");
            DefaultHttpContext httpContext = CreateHttpContext();
            ErrorHandlerMiddleware middleware = CreateMiddleware(nextDelegate);

            await middleware.Invoke(httpContext);

            Assert.AreEqual((int)HttpStatusCode.BadRequest, httpContext.Response.StatusCode);
            Assert.AreEqual("application/json", httpContext.Response.ContentType);
        }

        [TestMethod]
        public async Task Invoke_KeyNotFoundExceptionLanzada_RetornaStatusCodeNotFound()
        {
            RequestDelegate nextDelegate = _ => throw new KeyNotFoundException("Llave no encontrada");
            DefaultHttpContext httpContext = CreateHttpContext();
            ErrorHandlerMiddleware middleware = CreateMiddleware(nextDelegate);

            await middleware.Invoke(httpContext);

            Assert.AreEqual((int)HttpStatusCode.NotFound, httpContext.Response.StatusCode);
        }

        [TestMethod]
        public async Task Invoke_UnauthorizedAccessExceptionLanzada_RetornaStatusCodeUnauthorized()
        {
            RequestDelegate nextDelegate = _ => throw new UnauthorizedAccessException("Sin permisos");
            DefaultHttpContext httpContext = CreateHttpContext();
            ErrorHandlerMiddleware middleware = CreateMiddleware(nextDelegate);

            await middleware.Invoke(httpContext);

            Assert.AreEqual((int)HttpStatusCode.Unauthorized, httpContext.Response.StatusCode);
        }

        [TestMethod]
        public async Task Invoke_HttpRequestExceptionLanzada_RetornaStatusCodeServiceUnavailable()
        {
            RequestDelegate nextDelegate = _ => throw new HttpRequestException();
            DefaultHttpContext httpContext = CreateHttpContext();
            ErrorHandlerMiddleware middleware = CreateMiddleware(nextDelegate);

            await middleware.Invoke(httpContext);

            Assert.AreEqual((int)HttpStatusCode.ServiceUnavailable, httpContext.Response.StatusCode);
        }

        [TestMethod]
        public async Task Invoke_TaskCanceledExceptionLanzada_RetornaStatusCodeRequestTimeout()
        {
            RequestDelegate nextDelegate = _ => throw new TaskCanceledException();
            DefaultHttpContext httpContext = CreateHttpContext();
            ErrorHandlerMiddleware middleware = CreateMiddleware(nextDelegate);

            await middleware.Invoke(httpContext);

            Assert.AreEqual((int)HttpStatusCode.RequestTimeout, httpContext.Response.StatusCode);
        }

        [TestMethod]
        public async Task Invoke_ExcepcionNoControlada_UsaExceptionMapperYRetornaInternalServerError()
        {
            Exception excepcionGenerica = new InvalidOperationException("Fallo inesperado");
            RequestDelegate nextDelegate = _ => throw excepcionGenerica;
            DefaultHttpContext httpContext = CreateHttpContext();

            Mock<IExceptionMapper> mapperMock = new Mock<IExceptionMapper>();
            mapperMock.Setup(m => m.Map(excepcionGenerica)).Returns(new ExceptionResponse
            {
                StatusCode = (int)HttpStatusCode.InternalServerError,
                Response = new Answers<string>
                {
                    RespuestaEstado = false,
                    Mensaje = excepcionGenerica.Message,
                    CodigoEstado = HttpStatusCode.InternalServerError
                }
            });

            ErrorHandlerMiddleware middleware = new ErrorHandlerMiddleware(nextDelegate, _loggerMock.Object, mapperMock.Object);

            await middleware.Invoke(httpContext);

            Assert.AreEqual((int)HttpStatusCode.InternalServerError, httpContext.Response.StatusCode);
            mapperMock.Verify(m => m.Map(excepcionGenerica), Times.Once);
        }

        private ErrorHandlerMiddleware CreateMiddleware(RequestDelegate next) =>
            new ErrorHandlerMiddleware(next, _loggerMock.Object, _exceptionMapper);

        private static DefaultHttpContext CreateHttpContext()
        {
            DefaultHttpContext httpContext = new DefaultHttpContext();
            httpContext.Response.Body = new MemoryStream();
            return httpContext;
        }
    }
}
