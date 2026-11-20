using Conciliaciones.Dominio.Envoltorios;
using MediatR;

namespace Conciliaciones.Aplicacion.Features.ExternalAPIs.Querys.GetParameterizationsByClient;

/// <summary>
/// Único GET: consulta atada a proyectoId / procesoId / subProcesoId (el más específico gana).
/// </summary>
public record GetParameterizationsByClientQuery(
    string? ProyectoId,
    string? ProcesoId,
    string? SubProcesoId
) : IRequest<Answers<GetParameterizationsByClientResponse>>;
