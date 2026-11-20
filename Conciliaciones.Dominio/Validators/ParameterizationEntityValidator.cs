using Conciliaciones.Dominio.Entities.Parameterization;
using FluentValidation;

namespace Conciliaciones.Dominio.Validators
{
    public sealed class ParameterizationEntityValidator : AbstractValidator<ParameterizationEntity>
    {
        public ParameterizationEntityValidator()
        {
            RuleFor(x => x.ParametrizacionId).NotEmpty().MaximumLength(64);
            RuleFor(x => x.SubProcesoId).NotEmpty().MaximumLength(64);

            RuleForEach(x => x.Fuentes).ChildRules(fuente =>
            {
                fuente.RuleFor(f => f.FuenteId).MaximumLength(128);
                fuente.RuleFor(f => f.Tabla).MaximumLength(256);
            });

            When(x => x.Reglas != null, () =>
            {
                RuleForEach(x => x.Reglas).ChildRules(regla =>
                {
                    regla.RuleFor(r => r.NombreRegla).NotEmpty();
                });
            });
        }
    }
}
