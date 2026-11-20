using FluentValidation;
using FluentValidation.Results;

namespace Conciliaciones.Dominio.Validators
{
    public static class EntityValidator
    {
        public static void ValidateAndThrow<T>(T instance, IValidator<T> validator)
        {
            ValidationResult result = validator.Validate(instance);
            if (!result.IsValid)
            {
                throw new Exceptions.ValidationException(result.Errors);
            }
        }
    }
}
