using Conciliaciones.Dominio.Entities.Parameterization;
using FluentValidation;

namespace Conciliaciones.Dominio.Validators
{
    public sealed class ParameterizationUIEntityValidator : AbstractValidator<ParameterizationUIEntity>
    {
        public ParameterizationUIEntityValidator()
        {
            RuleFor(x => x.IdParametrizacion).NotEmpty().MaximumLength(64);
            RuleForEach(x => x.Fuente).SetValidator(new UiColumnEntityValidator());
            RuleForEach(x => x.Datos).ChildRules(block =>
            {
                block.RuleFor(b => b.IdFuente).NotEmpty();
                block.RuleForEach(b => b.Columns).SetValidator(new UiColumnEntityValidator());
            });
        }
    }

    public sealed class UiColumnEntityValidator : AbstractValidator<UiColumnEntity>
    {
        public UiColumnEntityValidator()
        {
            RuleFor(x => x.Field).NotEmpty().MaximumLength(256);
            RuleFor(x => x.Header).NotEmpty().MaximumLength(256);
            RuleFor(x => x.Order).GreaterThanOrEqualTo(0);
            RuleFor(x => x.CellConfig.Type).NotEmpty();
        }
    }
}
