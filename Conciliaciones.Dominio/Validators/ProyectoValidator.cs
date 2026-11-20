using Conciliaciones.Dominio.Entities.Client;
using FluentValidation;

namespace Conciliaciones.Dominio.Validators
{
    public sealed class ProyectoValidator : AbstractValidator<Proyecto>
    {
        public ProyectoValidator()
        {
            RuleFor(x => x.IdProyecto).NotEmpty().MaximumLength(64);
            RuleFor(x => x.Nombre).NotEmpty().MaximumLength(200);
        }
    }
}
