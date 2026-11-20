using Conciliaciones.Aplicacion.Ports.Interfaces;
using Conciliaciones.Dominio.DTOs.Response.Conciliation;
using Conciliaciones.Dominio.Envoltorios;
using MediatR;

namespace Conciliaciones.Aplicacion.Features.Conciliation.Commands.UpdateConciliacionEstado
{
    public class UpdateConciliacionEstadoHandler : IRequestHandler<UpdateConciliacionEstadoCommand, Answers<UpdateConciliacionEstadoResponse>>
    {
        private readonly IConciliationStateService _conciliationStateService;

        public UpdateConciliacionEstadoHandler(IConciliationStateService conciliationStateService)
        {
            _conciliationStateService = conciliationStateService ?? throw new ArgumentNullException(nameof(conciliationStateService), "El repositorio de conciliaciones no puede ser nulo.");
        }

        public async Task<Answers<UpdateConciliacionEstadoResponse>> Handle(UpdateConciliacionEstadoCommand request, CancellationToken cancellationToken)
        {
            if (request == null)
            {
                throw new ArgumentNullException(nameof(request), "La solicitud no puede ser nula.");
            }
            StatusUpdateConciliation sendData = await _conciliationStateService.UpdateEstadoAsync(request.ConciliacionId, cancellationToken);

            UpdateConciliacionEstadoResponse response = new UpdateConciliacionEstadoResponse
            (
                actualizados: sendData.Actualizados,
                noEncontrados: sendData.NoEncontrados,
                totalProcesados: sendData.TotalProcesados
            );

            return new Answers<UpdateConciliacionEstadoResponse>(response);
        }
    }
}
