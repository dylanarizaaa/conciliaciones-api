using Conciliaciones.Dominio.DTOs.Response.SubProcess;
using Conciliaciones.Dominio.Envoltorios;
using Conciliaciones.Dominio.Ports.Interfaces.SubProcess;
using MediatR;

namespace Conciliaciones.Aplicacion.Features.SubProcess.Querys.GetSubProcessByIdProcess
{
    public class GetSubProcessByIdProcessHandler : IRequestHandler<GetSubProcessByIdProcessQuery, Answers<GetSubProcessByIdProcessResponse>>
    {
        private readonly ISubProcessRepository _subProcessRepository;

        public GetSubProcessByIdProcessHandler(ISubProcessRepository subProcessRepository)
        {
            _subProcessRepository = subProcessRepository ?? throw new ArgumentNullException(nameof(subProcessRepository), "El repositorio de conciliaciones no puede ser nulo."); ;
        }
        public async Task<Answers<GetSubProcessByIdProcessResponse>> Handle(GetSubProcessByIdProcessQuery request, CancellationToken cancellationToken)
        {
            if (request == null)
            {
                throw new ArgumentNullException(nameof(request), "La consulta no puede ser nula.");
            }
            List<SubProcessInfoDto> listaResumenes = await _subProcessRepository.GetSubProcesosByIdProcess(request.IdProcess, cancellationToken);

            decimal montoTotalGlobal = listaResumenes != null ? listaResumenes.Sum(p => p.Monto) : 0m;
            long cantidadTotalCasos = listaResumenes != null ? listaResumenes.Sum(p => p.Cantidad) : 0L;
            GetSubProcessByIdProcessResponse response = new GetSubProcessByIdProcessResponse(
                montoTotalGlobal,
                cantidadTotalCasos,
                listaResumenes ?? new List<SubProcessInfoDto>()
            );
            return new Answers<GetSubProcessByIdProcessResponse>(response);
        }

    }
}
