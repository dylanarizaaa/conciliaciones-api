using Conciliaciones.Dominio.Ports.Interfaces.Hierarchy;
using Conciliaciones.Infraestructura.Persistence.DocumentDb.Constants;
using Conciliaciones.Infraestructura.Persistence.Documents;
using Conciliaciones.Infraestructura.Persistence.Documents.Factories;
using Conciliaciones.Infraestructura.Ports.Interfaces.DBContext;
using MongoDB.Driver;

namespace Conciliaciones.Infraestructura.Persistence.Hierarchy.Strategies
{
    public sealed class SubProcesoHierarchyStrategy : MongoHierarchyEntityStrategyBase<SubProcesoDocument>
    {
        public SubProcesoHierarchyStrategy(IDocumentDBContext dbContext)
            : base(dbContext)
        {
        }

        public override HierarchyEntityKind Kind => HierarchyEntityKind.SubProceso;

        protected override string CollectionName => DocumentDBCollectionNames.MongoCollectionNames.SubProcesos;

        protected override FilterDefinition<SubProcesoDocument> BuildLookupFilter(string name, string? parentId)
        {
            if (string.IsNullOrWhiteSpace(parentId))
            {
                throw new ArgumentException("idProceso es requerido para resolver/crear un subproceso.", nameof(parentId));
            }

            return Builders<SubProcesoDocument>.Filter.And(
                Builders<SubProcesoDocument>.Filter.Eq(x => x.IdProceso, parentId),
                Builders<SubProcesoDocument>.Filter.Eq(x => x.Nombre, name));
        }

        protected override FilterDefinition<SubProcesoDocument> BuildIdFilter(string id) =>
            Builders<SubProcesoDocument>.Filter.Eq(x => x.IdSubProceso, id);

        protected override string ExtractId(SubProcesoDocument document) => document.IdSubProceso;

        protected override SubProcesoDocument CreateDocument(string newId, string name, string? parentId) =>
            HierarchyDocumentFactory.CreateSubProceso(newId, name, parentId!);
    }
}
