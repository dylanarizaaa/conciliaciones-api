using FluentValidation;
using FluentValidation.Results;
using MediatR;
using DomainValidationException = Conciliaciones.Dominio.Exceptions.ValidationException;

namespace Conciliaciones.Aplicacion.Common.Behaviors
{
    public class ValidationBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
        where TRequest : IRequest<TResponse>
    {
        private readonly IEnumerable<IValidator<TRequest>> _validators;

        public ValidationBehavior(IEnumerable<IValidator<TRequest>> validators)
        {
            _validators = validators ?? throw new ArgumentNullException(nameof(validators), "La colección de validadores no puede ser nula.");
        }

        public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (!_validators.Any())
            {
                return await next();
            }

            ValidationContext<TRequest> context = new ValidationContext<TRequest>(request);

            ValidationResult[] validationResults = await Task.WhenAll(
                _validators.Select(validator => validator.ValidateAsync(context, cancellationToken)));

            List<ValidationFailure> failures = validationResults
                .SelectMany(result => result.Errors)
                .Where(failure => failure != null)
                .ToList();

            if (failures.Count != 0)
            {
                // Dominio.ValidationException: el middleware/ExceptionMapper mapean Errors y 400.
                throw new DomainValidationException(failures);
            }

            return await next();
        }
    }
}