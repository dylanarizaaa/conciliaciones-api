using Conciliaciones.Dominio.Entities.Client;
using FluentValidation;

namespace Conciliaciones.Dominio.Validators
{
    public sealed class SubProcesoValidator : AbstractValidator<SubProceso>
    {
        public SubProcesoValidator()
        {
            RuleFor(x => x.IdSubProceso).NotEmpty().MaximumLength(64);
            RuleFor(x => x.IdProceso).NotEmpty().MaximumLength(64);
            RuleFor(x => x.Nombre).NotEmpty().MaximumLength(200);
        }
    }
}
