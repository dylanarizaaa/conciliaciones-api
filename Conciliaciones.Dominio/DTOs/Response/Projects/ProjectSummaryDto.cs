namespace Conciliaciones.Dominio.DTOs.Response.Projects;

public record ProjectSummaryDto
(
    string ProyectoId,
    string NombreProyecto,
    decimal Monto,
    long Cantidad
);


