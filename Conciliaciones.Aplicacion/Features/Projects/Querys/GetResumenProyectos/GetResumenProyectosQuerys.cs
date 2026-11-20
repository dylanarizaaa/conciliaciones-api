using Conciliaciones.Dominio.Envoltorios;
using MediatR;

namespace Conciliaciones.Aplicacion.Features.Projects.Querys.GetResumenProyectos;

public record GetResumenProyectosQuery : IRequest<Answers<GetResumenProyectosResponse>>;
