using Conciliaciones.Dominio.Envoltorios;
using Conciliaciones.Dominio.Ports.Interfaces.Automatizaciones;
using MediatR;
using System.Net;

namespace Conciliaciones.Aplicacion.Features.Automatizaciones.Commands.CreateAutomatizacion
{
    public sealed class CreateAutomatizacionHandler
        : IRequestHandler<CreateAutomatizacionCommand, Answers<CreateAutomatizacionResponse>>
    {
        private readonly IAutomatizacionWritePort _writePort;

        public CreateAutomatizacionHandler(IAutomatizacionWritePort writePort)
        {
            _writePort = writePort ?? throw new ArgumentNullException(nameof(writePort));
        }

        public async Task<Answers<CreateAutomatizacionResponse>> Handle(
            CreateAutomatizacionCommand command,
            CancellationToken cancellationToken)
        {
            if (command?.Request == null)
            {
                throw new ArgumentNullException(nameof(command), "La solicitud no puede ser nula.");
            }

            (HttpStatusCode status, Dominio.DTOs.Response.Automatizaciones.CreateAutomatizacionResponse? data, string mensaje) =
                await _writePort.CreateAsync(command.Request, cancellationToken);

            if (data == null)
            {
                return new Answers<CreateAutomatizacionResponse>(mensaje, status);
            }

            CreateAutomatizacionResponse response = new(
                data.AutomatizacionId,
                data.ParametrizacionId,
                data.Mensaje);

            return new Answers<CreateAutomatizacionResponse>(response) { CodigoEstado = status, Mensaje = mensaje };
        }
    }
}
