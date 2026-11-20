using Conciliaciones.Dominio.DTOs.Requests.Conciliation;
using Conciliaciones.Dominio.Envoltorios;
using MediatR;

namespace Conciliaciones.Aplicacion.Features.Conciliation.Commands.CreateParametrizacionFull
{
    public record CreateParametrizacionFullCommand(
       CreateParametrizacionRequest Request
   ) : IRequest<Answers<CreateParametrizacionFullResponse>>;

}
