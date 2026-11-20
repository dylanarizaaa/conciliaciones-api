namespace Conciliaciones.Aplicacion.Features.Conciliation.Commands.CreateParametrizacionFull
{
    public record CreateParametrizacionFullResponse(
           string IdProyecto,
           string IdProceso,
           string IdSubProceso,
           string ParametrizacionId,
           string Mensaje
       );
}
