using Conciliaciones.Dominio.Envoltorios;
using Conciliaciones.Dominio.Ports.Interfaces.Conciliations;
using MediatR;
using System.Net;
using System.Text.Json;

namespace Conciliaciones.Aplicacion.Features.Conciliation.Querys.GetParametrizacionPlantilla
{
    public sealed class GetParametrizacionPlantillaHandler
        : IRequestHandler<GetParametrizacionPlantillaQuery, Answers<GetParametrizacionPlantillaResponse>>
    {
        private static readonly HashSet<string> TiposValidos = new(StringComparer.OrdinalIgnoreCase)
        {
            "table",
            "accordion"
        };

        private readonly IParametrizacionPlantillaQueryPort _queryPort;

        public GetParametrizacionPlantillaHandler(IParametrizacionPlantillaQueryPort queryPort)
        {
            _queryPort = queryPort ?? throw new ArgumentNullException(nameof(queryPort));
        }

        public async Task<Answers<GetParametrizacionPlantillaResponse>> Handle(GetParametrizacionPlantillaQuery request, CancellationToken cancellationToken)
        {
            if (request == null)
            {
                throw new ArgumentNullException(nameof(request));
            }
            string tipoBlock = request.TipoBlock?.Trim().ToLowerInvariant() ?? string.Empty;
            if (!TiposValidos.Contains(tipoBlock))
            {
                return new Answers<GetParametrizacionPlantillaResponse>("tipoBlock debe ser table o accordion.", HttpStatusCode.BadRequest);
            }

            string? json = await _queryPort.GetPlantillaJsonAsync(request?.ReglaId?.Trim(), tipoBlock, cancellationToken);

            if (string.IsNullOrWhiteSpace(json))
            {
                return new Answers<GetParametrizacionPlantillaResponse>("No existe plantilla para la regla y el tipo indicados.", HttpStatusCode.NotFound);
            }

            using JsonDocument document = JsonDocument.Parse(json);
            return new Answers<GetParametrizacionPlantillaResponse>(new GetParametrizacionPlantillaResponse(document.RootElement.Clone()));
        }
    }
}
