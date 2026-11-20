using System.Globalization;

namespace Conciliaciones.Dominio.Services
{
    public sealed class TransformacionMontoCalculator : ITransformacionMontoCalculator
    {
        public decimal Sum(IEnumerable<decimal> valores)
        {
            if (valores == null)
            {
                return 0m;
            }

            decimal total = 0m;
            foreach (decimal valor in valores)
            {
                total += valor;
            }

            return total;
        }

        public decimal ParseAmount(object? raw)
        {
            if (raw == null)
            {
                return 0m;
            }

            if (raw is decimal decimalValue)
            {
                return decimalValue;
            }

            if (raw is string texto
                && decimal.TryParse(texto, NumberStyles.Any, CultureInfo.InvariantCulture, out decimal parsedFromString))
            {
                return parsedFromString;
            }

            if (raw is double doble)
            {
                return Convert.ToDecimal(doble, CultureInfo.InvariantCulture);
            }

            if (raw is float flotante)
            {
                return Convert.ToDecimal(flotante, CultureInfo.InvariantCulture);
            }

            string? representado = raw.ToString();
            if (!string.IsNullOrWhiteSpace(representado)
                && decimal.TryParse(representado, NumberStyles.Any, CultureInfo.InvariantCulture, out decimal parsedFromToString))
            {
                return parsedFromToString;
            }

            try
            {
                return Convert.ToDecimal(raw, CultureInfo.InvariantCulture);
            }
            catch (Exception)
            {
                return 0m;
            }
        }
    }
}
