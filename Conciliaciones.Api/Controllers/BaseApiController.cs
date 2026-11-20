using MediatR;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics.CodeAnalysis;

namespace Conciliaciones.Api.Controllers
{
    [ExcludeFromCodeCoverage]
    [Route("/ms-conciliaciones/api/v1/[controller]")]
    [ApiController]
    public class BaseApiController : ControllerBase
    {
        private IMediator? _mediator;

        protected IMediator Mediator => _mediator ??= HttpContext.RequestServices.GetService<IMediator>() ?? throw new InvalidOperationException("El servicio IMediator no está registrado en el contenedor de dependencias.");
    }
}
