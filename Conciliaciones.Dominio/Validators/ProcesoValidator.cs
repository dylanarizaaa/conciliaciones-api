using Conciliaciones.Dominio.Entities.Client;
using FluentValidation;

namespace Conciliaciones.Dominio.Validators
{
    public sealed class ProcesoValidator : AbstractValidator<Proceso>
    {
        public ProcesoValidator()
        {
            RuleFor(x => x.IdProceso).NotEmpty().MaximumLength(64);
            RuleFor(x => x.IdProyecto).NotEmpty().MaximumLength(64);
            RuleFor(x => x.Nombre).NotEmpty().MaximumLength(200);
        }
    }
}
