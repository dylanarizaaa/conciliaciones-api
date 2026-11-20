using Conciliaciones.Aplicacion.Ports.Interfaces;
using Conciliaciones.Dominio.DTOs.Response.Conciliation;
using Conciliaciones.Dominio.Ports.Interfaces.Conciliations;

namespace Conciliaciones.Aplicacion.Services
{
    public sealed class ConciliationQueryService : IConciliationQueryService
    {
        private readonly IConciliationQueryPort _queryPort;

        public ConciliationQueryService(IConciliationQueryPort queryPort)
        {
            _queryPort = queryPort ?? throw new ArgumentNullException(nameof(queryPort));
        }

        public Task<ColumsDetailsDto> GetConciliationByIdSp(string reglaId, CancellationToken cancellationToken)
        {
            return _queryPort.GetConciliationByIdSp(reglaId, cancellationToken);
        }

        public Task<PagedRowsDto> GetConciliationRowsAsync(string reglaId, int pageNumber, int pageSize, CancellationToken cancellationToken)
        {
            return _queryPort.GetConciliationRowsAsync(reglaId, pageNumber, pageSize, cancellationToken);
        }

        public Task<List<FuenteDetalleDto>> GetConciliationFuentesDetailAsync(string conciliacionId, CancellationToken cancellationToken)
        {
            return _queryPort.GetConciliationFuentesDetailAsync(conciliacionId, cancellationToken);
        }
    }
}
