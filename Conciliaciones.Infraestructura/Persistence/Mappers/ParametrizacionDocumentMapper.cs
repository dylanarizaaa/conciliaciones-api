using Conciliaciones.Dominio.DTOs.Requests.Conciliation;
using Conciliaciones.Infraestructura.Persistence.Documents;
using Conciliaciones.Infraestructura.Persistence.Documents.Factories;

namespace Conciliaciones.Infraestructura.Persistence.Mappers
{
    public static class ParametrizacionDocumentMapper
    {
        public static ParametrizacionDocument FromRequest(
            ParametrizacionBodyDto? body,
            string parametrizacionId,
            string subProcesoId) =>
            ParametrizacionDocumentFactory.FromRequest(body, parametrizacionId, subProcesoId);
    }
}
