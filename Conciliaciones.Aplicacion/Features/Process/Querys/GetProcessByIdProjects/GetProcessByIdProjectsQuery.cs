using Conciliaciones.Dominio.Envoltorios;
using MediatR;

namespace Conciliaciones.Aplicacion.Features.Process.Querys.GetProcessByIdProjects;

public record GetProcessByIdProjectsQuery(string ProyectoId)
     : IRequest<Answers<GetProcessByIdProjectsResponse>>;

