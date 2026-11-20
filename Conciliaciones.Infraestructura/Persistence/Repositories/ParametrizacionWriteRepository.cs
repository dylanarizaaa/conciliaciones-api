using Conciliaciones.Dominio.DTOs.Requests.Conciliation;
using Conciliaciones.Dominio.DTOs.Response.Conciliation;
using Conciliaciones.Dominio.Ports.Interfaces.Conciliations;
using Conciliaciones.Dominio.Ports.Interfaces.Hierarchy;
using Conciliaciones.Infraestructura.Persistence.DocumentDb.Constants;
using Conciliaciones.Infraestructura.Persistence.Documents;
using Conciliaciones.Infraestructura.Persistence.Mappers;
using Conciliaciones.Infraestructura.Ports.Interfaces.DBContext;
using MongoDB.Driver;

namespace Conciliaciones.Infraestructura.Persistence.Repositories
{
    public class ParametrizacionWriteRepository : IParametrizacionWritePort
    {
        private readonly IDocumentDBContext _dbContext;
        private readonly IHierarchyWritePort _hierarchyWritePort;

        public ParametrizacionWriteRepository(IDocumentDBContext documentDBContext, IHierarchyWritePort hierarchyWritePort)
        {
            _dbContext = documentDBContext ?? throw new ArgumentNullException(nameof(documentDBContext));
            _hierarchyWritePort = hierarchyWritePort ?? throw new ArgumentNullException(nameof(hierarchyWritePort));
        }

        public async Task<CreateParametrizacionResponse> CreateParametrizacionAsync(CreateParametrizacionRequest request, CancellationToken cancellationToken)
        {
            string idProyecto = await _hierarchyWritePort.ResolveOrCreateProyectoAsync(request.proyecto!, cancellationToken);
            string idProceso = await _hierarchyWritePort.ResolveOrCreateProcesoAsync(request.proceso!, idProyecto, cancellationToken);
            string idSubProceso = await _hierarchyWritePort.ResolveOrCreateSubProcesoAsync(request.subproceso!, idProceso, cancellationToken);

            string parametrizacionId = Guid.NewGuid().ToString();
            ParametrizacionDocument parametrizacionDoc = ParametrizacionDocumentMapper.FromRequest(request.parametrizacion, parametrizacionId, idSubProceso);

            IMongoCollection<ParametrizacionDocument> parametrizacionCol = _dbContext.GetCollection<ParametrizacionDocument>(DocumentDBCollectionNames.MongoCollectionNames.Parametrizacion);
            await parametrizacionCol.InsertOneAsync(parametrizacionDoc, cancellationToken: cancellationToken);

            try
            {
                ParametrizacionUiDocument uiDoc = ParametrizacionUiDocumentFactory.Build(parametrizacionId, request.parametrizacionUI);
                IMongoCollection<ParametrizacionUiDocument> parametrizacionUiCol = _dbContext.GetCollection<ParametrizacionUiDocument>(
                DocumentDBCollectionNames.MongoCollectionNames.ParametrizacionUI);
                await parametrizacionUiCol.InsertOneAsync(uiDoc, cancellationToken: cancellationToken);
            } catch
            {
                // Compensating delete: avoid orphan parametrización when UI insert fails (no multi-doc txn required).
                await parametrizacionCol.DeleteOneAsync(Builders<ParametrizacionDocument>.Filter.Eq(x => x.ParametrizacionId, parametrizacionId), cancellationToken);
                throw;
            }

            return new CreateParametrizacionResponse(
                idProyecto,
                idProceso,
                idSubProceso,
                parametrizacionId,
                "Parametrización creada y entidades vinculadas correctamente."
            );
        }
    }
}
