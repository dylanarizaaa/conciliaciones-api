using FluentValidation.Results;

namespace Conciliaciones.Dominio.Exceptions
{
    public class ValidationException : Exception
    {
        public List<string> Errors { get; }
        public ValidationException() : base("Se han producido uno o más errores de validación.")
        {
            Errors = new List<string>();
        }

        public ValidationException(IEnumerable<ValidationFailure> failures) : this()
        {
            if (failures != null)
            {
                foreach (ValidationFailure failure in failures)
                {
                    if (failure != null && !string.IsNullOrWhiteSpace(failure.ErrorMessage))
                    {
                        Errors.Add(failure.ErrorMessage);
                    }
                }
            }
        }
    }
}
