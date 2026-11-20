using Conciliaciones.Api.Controllers.v1;
using Conciliaciones.Api.Middlewares;
using Conciliaciones.Dominio.DTOs.Response.Middlewares;
using Conciliaciones.Dominio.Envoltorios;
using Conciliaciones.Dominio.Ports.Interfaces.Middleware;
using Conciliaciones.Dominio.Ports.Interfaces.Persistence;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using System.Net;
using System.Text;

namespace Conciliaciones.Pruebas.Conciliaciones.Api.NUnit
{
    [TestFixture]
    public class ApiCoverageBoostTests
    {
        [Test]
        public async Task TestConnection_Development_HappyPaths()
        {
            Mock<IDatabaseProbe> probe = new();
            Mock<IHostEnvironment> env = new();
            env.SetupGet(e => e.EnvironmentName).Returns(Environments.Development);

            probe.Setup(p => p.PingAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(new DatabasePingResult(true, "ok", "{}", "Conciliaciones", new List<string> { "a" }, null, null));
            probe.Setup(p => p.ListDatabasesAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(new DatabaseCatalogResult(true, "OK", new List<string> { "Conciliaciones" }, "Conciliaciones", null, null));

            TestConnectionController controller = new(probe.Object, env.Object);

            IActionResult mongo = await controller.CheckMongoConnection(CancellationToken.None);
            mongo.Should().BeOfType<OkObjectResult>();

            IActionResult dbs = await controller.CheckDatabases(CancellationToken.None);
            dbs.Should().BeOfType<OkObjectResult>();
        }

        [Test]
        public async Task ErrorHandler_ClientAbort_DoesNotWriteBody()
        {
            Mock<IExceptionMapper> mapper = new();
            Mock<ILogger<ErrorHandlerMiddleware>> logger = new();
            ErrorHandlerMiddleware mw = new(
                _ => throw new OperationCanceledException(),
                logger.Object,
                mapper.Object);

            DefaultHttpContext ctx = new();
            CancellationTokenSource cts = new();
            cts.Cancel();
            ctx.RequestAborted = cts.Token;

            await mw.Invoke(ctx);
            mapper.Verify(m => m.Map(It.IsAny<Exception>()), Times.Never);
        }

        [Test]
        public async Task ErrorHandler_HasStarted_SkipsWrite()
        {
            Mock<IExceptionMapper> mapper = new();
            mapper.Setup(m => m.Map(It.IsAny<Exception>())).Returns(new ExceptionResponse
            {
                StatusCode = (int)HttpStatusCode.InternalServerError,
                Response = new Answers<string> { Mensaje = "x", CodigoEstado = HttpStatusCode.InternalServerError }
            });
            Mock<ILogger<ErrorHandlerMiddleware>> logger = new();

            ErrorHandlerMiddleware mw = new(
                _ => throw new InvalidOperationException("boom"),
                logger.Object,
                mapper.Object);

            DefaultHttpContext ctx = new();
            MemoryStream body = new();
            ctx.Features.Set<Microsoft.AspNetCore.Http.Features.IHttpResponseFeature>(new StartedHttpResponseFeature(body));
            ctx.Features.Set<Microsoft.AspNetCore.Http.Features.IHttpResponseBodyFeature>(
                new Microsoft.AspNetCore.Http.StreamResponseBodyFeature(body));

            await mw.Invoke(ctx);
            mapper.Verify(m => m.Map(It.IsAny<Exception>()), Times.Once);
            body.Length.Should().Be(0);
        }

        private sealed class StartedHttpResponseFeature : Microsoft.AspNetCore.Http.Features.IHttpResponseFeature
        {
            public StartedHttpResponseFeature(Stream body) => Body = body;
            public int StatusCode { get; set; } = 200;
            public string? ReasonPhrase { get; set; }
            public IHeaderDictionary Headers { get; set; } = new HeaderDictionary();
            public Stream Body { get; set; }
            public bool HasStarted => true;
            public void OnStarting(Func<object, Task> callback, object state) { }
            public void OnCompleted(Func<object, Task> callback, object state) { }
        }
    }
}
