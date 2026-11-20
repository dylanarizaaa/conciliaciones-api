using Conciliaciones.Dominio.Ports.Interfaces.Hierarchy;
using Conciliaciones.Infraestructura.Persistence.DocumentDb.Constants;
using Conciliaciones.Infraestructura.Persistence.Documents;
using Conciliaciones.Infraestructura.Persistence.Documents.Factories;
using Conciliaciones.Infraestructura.Ports.Interfaces.DBContext;
using MongoDB.Driver;

namespace Conciliaciones.Infraestructura.Persistence.Hierarchy.Strategies
{
    public sealed class ProyectoHierarchyStrategy : MongoHierarchyEntityStrategyBase<ProyectoDocument>
    {
        public ProyectoHierarchyStrategy(IDocumentDBContext dbContext)
            : base(dbContext)
        {
        }

        public override HierarchyEntityKind Kind => HierarchyEntityKind.Proyecto;

        protected override string CollectionName => DocumentDBCollectionNames.MongoCollectionNames.Proyectos;

        protected override FilterDefinition<ProyectoDocument> BuildLookupFilter(string name, string? parentId) =>
            Builders<ProyectoDocument>.Filter.Eq(x => x.Nombre, name);

        protected override FilterDefinition<ProyectoDocument> BuildIdFilter(string id) =>
            Builders<ProyectoDocument>.Filter.Eq(x => x.IdProyecto, id);

        protected override string ExtractId(ProyectoDocument document) =>
            !string.IsNullOrWhiteSpace(document.IdProyecto)
                ? document.IdProyecto
                : document.Id.ToString();

        protected override ProyectoDocument CreateDocument(string newId, string name, string? parentId) =>
            HierarchyDocumentFactory.CreateProyecto(newId, name);
    }
}
