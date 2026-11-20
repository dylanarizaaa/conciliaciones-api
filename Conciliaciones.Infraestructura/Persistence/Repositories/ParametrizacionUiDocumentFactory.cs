using Conciliaciones.Dominio.DTOs.Requests.Conciliation;
using Conciliaciones.Infraestructura.Persistence.Documents;
using Conciliaciones.Infraestructura.Persistence.Documents.Factories;

namespace Conciliaciones.Infraestructura.Persistence.Repositories
{
    /// <summary>Fachada de compatibilidad hacia <see cref="Documents.Factories.ParametrizacionUiDocumentFactory"/>.</summary>
    public static class ParametrizacionUiDocumentFactory
    {
        public static ParametrizacionUiDocument Build(string parametrizacionId, parametrizacionUiBodyDto? uiInput) =>
            Documents.Factories.ParametrizacionUiDocumentFactory.FromRequest(parametrizacionId, uiInput);
    }
}
