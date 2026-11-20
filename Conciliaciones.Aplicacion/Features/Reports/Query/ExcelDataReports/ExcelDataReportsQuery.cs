using Conciliaciones.Dominio.Envoltorios;
using MediatR;

namespace Conciliaciones.Aplicacion.Features.Reports.Query.ExcelDataReports
{
    /// <summary>
    /// Export Excel por reglaId (misma clave que subprocesos-registros/reglas).
    /// </summary>
    public record ExcelDataReportsQuery(
        string ReglaId
    ) : IRequest<Answers<ExcelDataReportsResponse>>;
}
