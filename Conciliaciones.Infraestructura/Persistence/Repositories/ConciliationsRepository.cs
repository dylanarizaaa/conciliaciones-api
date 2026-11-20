using Conciliaciones.Dominio.DTOs.Requests.Conciliation;
using Conciliaciones.Dominio.DTOs.Response.Conciliation;
using Conciliaciones.Dominio.Ports.Interfaces.Conciliations;

namespace Conciliaciones.Infraestructura.Persistence.Repositories
{
    /// <summary>
    /// Fachada sobre puertos de query/state/write. Un solo ctor para DI (evitar constructores ambiguos).
    /// </summary>
    public class ConciliationsRepository : IConciliationsRepository
    {
        private readonly IConciliationQueryPort _queryPort;
        private readonly IConciliationStatePort _statePort;
        private readonly IParametrizacionWritePort _writePort;

        public ConciliationsRepository(
            IConciliationQueryPort queryPort,
            IConciliationStatePort statePort,
            IParametrizacionWritePort writePort)
        {
            _queryPort = queryPort ?? throw new ArgumentNullException(nameof(queryPort));
            _statePort = statePort ?? throw new ArgumentNullException(nameof(statePort));
            _writePort = writePort ?? throw new ArgumentNullException(nameof(writePort));
        }

        public Task<ColumsDetailsDto> GetConciliationByIdSp(string reglaId, CancellationToken cancellationToken) =>
            _queryPort.GetConciliationByIdSp(reglaId, cancellationToken);

        public Task<PagedRowsDto> GetConciliationRowsAsync(string reglaId, int pageNumber, int pageSize, CancellationToken cancellationToken) => _queryPort.GetConciliationRowsAsync(reglaId, pageNumber, pageSize, cancellationToken);

        public Task<List<FuenteDetalleDto>> GetConciliationFuentesDetailAsync(string conciliacionId, CancellationToken cancellationToken) =>
            _queryPort.GetConciliationFuentesDetailAsync(conciliacionId, cancellationToken);

        public Task<StatusUpdateConciliation> UpdateEstadoConciliacionAsync(
            List<Guid> conciliacionId,
            CancellationToken cancellationToken) =>
            _statePort.UpdateEstadoConciliacionAsync(conciliacionId, cancellationToken);

        public Task<CreateParametrizacionResponse> CreateParametrizacionAsync(
            CreateParametrizacionRequest request,
            CancellationToken cancellationToken) =>
            _writePort.CreateParametrizacionAsync(request, cancellationToken);
    }
}
