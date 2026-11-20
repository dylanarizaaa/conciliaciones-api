using Conciliaciones.Dominio.Services;

namespace Conciliaciones.Infraestructura.Persistence.Mappers
{
    public static class TransformacionMontoReader
    {
        public static decimal SumFromConciliaciones(
            IEnumerable<Dictionary<string, object>?>? transformaciones,
            ITransformacionMontoCalculator calculator)
        {
            if (transformaciones == null || calculator == null)
            {
                return 0m;
            }

            List<decimal> montos = new List<decimal>();
            foreach (Dictionary<string, object>? transformacion in transformaciones)
            {
                if (transformacion == null)
                {
                    continue;
                }

                montos.AddRange(ReadAmounts(transformacion, calculator));
            }

            return calculator.Sum(montos);
        }

        public static decimal SumFromConciliacionMontos(
            IEnumerable<Documents.Projections.ConciliacionMontoProjection>? conciliaciones,
            ITransformacionMontoCalculator calculator)
        {
            return SumFromConciliaciones(
                conciliaciones?.Select(c => c.ResultadoTransformacion),
                calculator);
        }

        public static IEnumerable<decimal> ReadAmounts(
            Dictionary<string, object> resultadoTransformacion,
            ITransformacionMontoCalculator calculator)
        {
            foreach (KeyValuePair<string, object> element in resultadoTransformacion)
            {
                if (element.Value == null)
                {
                    continue;
                }

                yield return calculator.ParseAmount(element.Value);
            }
        }
    }
}
