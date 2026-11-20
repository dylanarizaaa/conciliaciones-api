using Conciliaciones.Dominio.Entities.Common;
using MongoDB.Bson;

namespace Conciliaciones.Infraestructura.Persistence.Documents.Factories
{
    /// <summary>
    /// Columna "Estado" de la tabla de la plantilla (pantalla Regla). Sus variantes son los estados
    /// reales de <see cref="ConciliacionEstados"/> y lee el campo que envía
    /// <c>subprocesos-registros/reglas</c> (<c>estadoConciliacion</c>).
    /// </summary>
    public static class PlantillaEstadoColumna
    {
        public const string Id = "estado";

        /// <summary>Campo de la fila con el estado (ver ConciliationRowsComposer).</summary>
        public const string AccessorKey = "estadoConciliacion";

        public static BsonDocument Build() =>
            new()
            {
                { "id", Id },
                { "accessorKey", AccessorKey },
                {
                    "header",
                    new BsonDocument
                    {
                        { "component", "SortIndicator" },
                        { "inputs", new BsonDocument("label", "Estado") }
                    }
                },
                { "cell", Cell() },
                { "meta", new BsonDocument("width", 206) },
                { "enableSorting", true },
                { "sortType", "text" },
                { "pin", "end" }
            };

        /// <summary>
        /// Las plantillas se guardan al crear la parametrización: esta corrección se aplica también al
        /// leerlas, para que las ya guardadas respondan con el campo y las variantes correctas.
        /// <paramref name="tabla"/> es <c>plantilla.table</c>.
        /// </summary>
        public static void Aplicar(BsonDocument tabla)
        {
            if (!tabla.TryGetValue("blocks", out BsonValue blocks) || !blocks.IsBsonArray)
            {
                return;
            }

            foreach (BsonValue block in blocks.AsBsonArray)
            {
                if (!block.IsBsonDocument
                    || !block.AsBsonDocument.TryGetValue("table", out BsonValue table)
                    || !table.IsBsonDocument
                    || !table.AsBsonDocument.TryGetValue("columns", out BsonValue columns)
                    || !columns.IsBsonArray)
                {
                    continue;
                }

                foreach (BsonValue column in columns.AsBsonArray)
                {
                    if (column.IsBsonDocument
                        && column.AsBsonDocument.TryGetValue("id", out BsonValue id)
                        && id.IsString
                        && id.AsString == Id)
                    {
                        column.AsBsonDocument["accessorKey"] = AccessorKey;
                        column.AsBsonDocument["cell"] = Cell();
                    }
                }
            }
        }

        private static BsonDocument Cell() =>
            new()
            {
                { "component", "BadgeCell" },
                {
                    "inputs",
                    new BsonDocument
                    {
                        {
                            "variants",
                            new BsonDocument
                            {
                                { ConciliacionEstados.con_novedad, Variante("Con novedad", "danger") },
                                { ConciliacionEstados.error_evaluacion, Variante("Error de evaluación", "danger") },
                                { ConciliacionEstados.datos_incompletos, Variante("Datos incompletos", "warning") },
                                { ConciliacionEstados.validando, Variante("Validando", "info") },
                                { ConciliacionEstados.conciliado, Variante("Conciliado", "success") }
                            }
                        },
                        { "fallback", Variante("Sin estado", "neutral") }
                    }
                }
            };

        private static BsonDocument Variante(string label, string tone) =>
            new()
            {
                { "label", label },
                { "tone", tone }
            };
    }
}
