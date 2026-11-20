using Conciliaciones.Dominio.DTOs.Response.Conciliation;

namespace Conciliaciones.Dominio.Ports.Interfaces.Conciliations
{
    public interface IConciliationQueryPort
    {
        Task<ColumsDetailsDto> GetConciliationByIdSp(string reglaId, CancellationToken cancellationToken);

        Task<PagedRowsDto> GetConciliationRowsAsync(string reglaId, int pageNumber, int pageSize, CancellationToken cancellationToken);

        Task<List<FuenteDetalleDto>> GetConciliationFuentesDetailAsync(string conciliacionId, CancellationToken cancellationToken);
    }
}
