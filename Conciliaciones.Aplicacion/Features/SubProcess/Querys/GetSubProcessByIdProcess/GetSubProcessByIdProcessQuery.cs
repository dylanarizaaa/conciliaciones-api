using Conciliaciones.Dominio.Envoltorios;
using MediatR;

namespace Conciliaciones.Aplicacion.Features.SubProcess.Querys.GetSubProcessByIdProcess;

public record GetSubProcessByIdProcessQuery(string IdProcess) : IRequest<Answers<GetSubProcessByIdProcessResponse>>;



