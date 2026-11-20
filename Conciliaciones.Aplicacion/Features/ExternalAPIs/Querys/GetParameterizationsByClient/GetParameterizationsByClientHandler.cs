using Conciliaciones.Dominio.DTOs.Response.ExternalAPIs;
using Conciliaciones.Dominio.Envoltorios;
using Conciliaciones.Dominio.Ports.Interfaces.ExternalAPIs;
using MediatR;
using System.Net;

namespace Conciliaciones.Aplicacion.Features.ExternalAPIs.Querys.GetParameterizationsByClient
{
    public sealed class GetParameterizationsByClientHandler
        : IRequestHandler<GetParameterizationsByClientQuery, Answers<GetParameterizationsByClientResponse>>
    {
        private readonly IExternalAPIsRepository _externalAPIsRepository;

        public GetParameterizationsByClientHandler(IExternalAPIsRepository externalAPIsRepository)
        {
            _externalAPIsRepository = externalAPIsRepository
                ?? throw new ArgumentNullException(nameof(externalAPIsRepository));
        }

        public async Task<Answers<GetParameterizationsByClientResponse>> Handle(
            GetParameterizationsByClientQuery request,
            CancellationToken cancellationToken)
        {
            if (request == null)
            {
                throw new ArgumentNullException(nameof(request));
            }

            bool sinFiltros = string.IsNullOrWhiteSpace(request.ProyectoId)
                && string.IsNullOrWhiteSpace(request.ProcesoId)
                && string.IsNullOrWhiteSpace(request.SubProcesoId);

            if (sinFiltros)
            {
                return new Answers<GetParameterizationsByClientResponse>(
                    "Debe informar proyectoId, procesoId o subProcesoId.",
                    HttpStatusCode.BadRequest);
            }

            IReadOnlyList<RulesParamsResponseDto> data = await _externalAPIsRepository.GetParameterizationsByClientAsync(
                request.ProyectoId,
                request.ProcesoId,
                request.SubProcesoId,
                cancellationToken);

            return new Answers<GetParameterizationsByClientResponse>(
                new GetParameterizationsByClientResponse(data.ToList()));
        }
    }
}
