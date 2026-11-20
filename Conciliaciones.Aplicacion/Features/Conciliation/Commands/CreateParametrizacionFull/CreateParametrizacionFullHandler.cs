using Conciliaciones.Aplicacion.Ports.Interfaces;
using Conciliaciones.Dominio.DTOs.Response.Conciliation;
using Conciliaciones.Dominio.Envoltorios;
using MediatR;

namespace Conciliaciones.Aplicacion.Features.Conciliation.Commands.CreateParametrizacionFull
{
    public class CreateParametrizacionFullCommandHandler
        : IRequestHandler<CreateParametrizacionFullCommand, Answers<CreateParametrizacionFullResponse>>
    {
        private readonly IParametrizacionApplicationService _parametrizacionApplicationService;

        public CreateParametrizacionFullCommandHandler(IParametrizacionApplicationService parametrizacionApplicationService)
        {
            _parametrizacionApplicationService = parametrizacionApplicationService ?? throw new ArgumentNullException(nameof(parametrizacionApplicationService), "El repositorio de parametrización no puede ser nulo.");
        }
        public async Task<Answers<CreateParametrizacionFullResponse>> Handle(CreateParametrizacionFullCommand command, CancellationToken cancellationToken)
        {
            if (command == null || command.Request == null)
            {
                throw new ArgumentNullException(nameof(command), "La solicitud para crear la parametrización no puede ser nula.");
            }
            CreateParametrizacionResponse dataResponse = await _parametrizacionApplicationService.CreateAsync(command.Request, cancellationToken);
            CreateParametrizacionFullResponse res = new CreateParametrizacionFullResponse(
                dataResponse.IdProyecto,
                dataResponse.IdProceso,
                dataResponse.IdSubProceso,
                dataResponse.ParametrizacionId,
                dataResponse.Mensaje
            );
            return new Answers<CreateParametrizacionFullResponse>(res);
        }
    }
}
