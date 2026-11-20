using Conciliaciones.Dominio.Ports.Interfaces.Hierarchy;
using Conciliaciones.Infraestructura.Persistence.DocumentDb.Constants;
using Conciliaciones.Infraestructura.Persistence.Documents;
using Conciliaciones.Infraestructura.Persistence.Documents.Factories;
using Conciliaciones.Infraestructura.Ports.Interfaces.DBContext;
using MongoDB.Driver;

namespace Conciliaciones.Infraestructura.Persistence.Hierarchy.Strategies
{
    public sealed class ProcesoHierarchyStrategy : MongoHierarchyEntityStrategyBase<ProcesoDocument>
    {
        public ProcesoHierarchyStrategy(IDocumentDBContext dbContext)
            : base(dbContext)
        {
        }

        public override HierarchyEntityKind Kind => HierarchyEntityKind.Proceso;

        protected override string CollectionName => DocumentDBCollectionNames.MongoCollectionNames.Procesos;

        protected override FilterDefinition<ProcesoDocument> BuildLookupFilter(string name, string? parentId)
        {
            if (string.IsNullOrWhiteSpace(parentId))
            {
                throw new ArgumentException("idProyecto es requerido para resolver/crear un proceso.", nameof(parentId));
            }

            return Builders<ProcesoDocument>.Filter.And(
                Builders<ProcesoDocument>.Filter.Eq(x => x.IdProyecto, parentId),
                Builders<ProcesoDocument>.Filter.Eq(x => x.Nombre, name));
        }

        protected override FilterDefinition<ProcesoDocument> BuildIdFilter(string id) =>
            Builders<ProcesoDocument>.Filter.Eq(x => x.IdProceso, id);

        protected override string ExtractId(ProcesoDocument document) => document.IdProceso;

        protected override ProcesoDocument CreateDocument(string newId, string name, string? parentId) =>
            HierarchyDocumentFactory.CreateProceso(newId, name, parentId!);
    }
}
