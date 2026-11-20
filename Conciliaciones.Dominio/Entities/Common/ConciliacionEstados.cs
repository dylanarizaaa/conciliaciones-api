using Conciliaciones.Dominio.Exceptions;
using FluentValidation.Results;

namespace Conciliaciones.Dominio.Entities.Common
{
    public static class ConciliacionEstados
    {
        public const string con_novedad = "CON_NOVEDAD";
        public const string validando = "VALIDANDO";
        public const string conciliado = "CONCILIADO";
        public const string datos_incompletos = "DATOS_INCOMPLETOS";
        public const string error_evaluacion = "ERROR_EVALUACION";


        public static readonly IReadOnlyList<string> FallidosOError = new[] { con_novedad, error_evaluacion, datos_incompletos, validando };
        public static readonly IReadOnlyList<string> FallidosOErrorComparacion = FallidosOError.Select(estado => estado.ToLowerInvariant()).ToArray();

        private static readonly string[] AllowedCanonical =
        {
            con_novedad,
            validando,
            datos_incompletos,
            error_evaluacion

        };

        public static string NormalizeOrThrow(string? nuevoEstado)
        {
            string raw = string.IsNullOrWhiteSpace(nuevoEstado) ? validando : nuevoEstado.Trim();
            string? canonical = AllowedCanonical.FirstOrDefault(a =>
                a.Equals(raw, StringComparison.OrdinalIgnoreCase));

            if (canonical == null)
            {
                throw new ValidationException(new[]
                {
                    new ValidationFailure(
                        nameof(nuevoEstado),
                        $"Estado '{raw}' no es válido. Permitidos: {string.Join(", ", AllowedCanonical)}.")
                });
            }

            return canonical;
        }
    }
}
