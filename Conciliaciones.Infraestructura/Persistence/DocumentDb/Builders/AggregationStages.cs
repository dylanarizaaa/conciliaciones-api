using Conciliaciones.Dominio.Entities.Common;
using Conciliaciones.Infraestructura.Persistence.DocumentDb.Constants;
using MongoDB.Bson;

namespace Conciliaciones.Infraestructura.Persistence.DocumentDb.Builders
{
    public static class AggregationStages
    {
        public static BsonDocument MatchEq(string field, BsonValue value) =>
            new BsonDocument("$match", new BsonDocument(field, value));

        public static BsonDocument Lookup(string from, string localField, string foreignField, string @as) =>
            new BsonDocument("$lookup", new BsonDocument
            {
                { "from", from },
                { "localField", localField },
                { "foreignField", foreignField },
                { "as", @as }
            });

        public static BsonDocument Unwind(string path, bool preserveNullAndEmptyArrays = true) =>
            new BsonDocument("$unwind", new BsonDocument
            {
                { "path", path },
                { "preserveNullAndEmptyArrays", preserveNullAndEmptyArrays }
            });

        public static BsonDocument[] LookupUnwind(
            string from,
            string localField,
            string foreignField,
            string @as,
            bool preserveNullAndEmptyArrays = true) =>
            new[]
            {
                Lookup(from, localField, foreignField, @as),
                Unwind($"${@as}", preserveNullAndEmptyArrays)
            };

        /// <summary>
        /// Match por reglas.idRegla y deja reglaSeleccionada para filtrar conciliaciones por nombreRegla.
        /// </summary>
        public static BsonDocument[] MatchReglaId(string reglaId) =>
            new[]
            {
                new BsonDocument("$match", new BsonDocument
                {
                    { $"{DocumentFields.Parametrizacion.Reglas}.{DocumentFields.Reglas.IdRegla}", reglaId }
                }),
                new BsonDocument("$addFields", new BsonDocument
                {
                    {
                        DocumentFields.AggregationAliases.ReglaSeleccionada,
                        new BsonDocument("$first", new BsonDocument("$filter", new BsonDocument
                        {
                            { "input", $"${DocumentFields.Parametrizacion.Reglas}" },
                            { "as", "r" },
                            {
                                "cond",
                                new BsonDocument("$eq", new BsonArray
                                {
                                    $"$$r.{DocumentFields.Reglas.IdRegla}",
                                    reglaId
                                })
                            }
                        }))
                    }
                })
            };

        /// <summary>
        /// Filtra conciliaciones fallidas/error cuya resultadoRegla.nombreRegla = reglaSeleccionada.nombreRegla.
        /// </summary>
        public static BsonDocument FilterFailedByReglaSeleccionada() =>
            new BsonDocument("$addFields", new BsonDocument
            {
                {
                    DocumentFields.AggregationAliases.ConciliacionesFiltradas,
                    new BsonDocument("$filter", new BsonDocument
                    {
                        { "input", $"${DocumentFields.AggregationAliases.Conciliaciones}" },
                        { "as", "c" },
                        {
                            "cond",
                            new BsonDocument("$and", new BsonArray
                            {
                                new BsonDocument("$eq", new BsonArray
                                {
                                    $"$$c.{DocumentFields.Conciliacion.ResultadoRegla}.{DocumentFields.Conciliacion.NombreRegla}",
                                    $"${DocumentFields.AggregationAliases.ReglaSeleccionada}.{DocumentFields.Reglas.NombreRegla}"
                                }),
                                new BsonDocument("$in", new BsonArray
                                {
                                    new BsonDocument("$toLower", new BsonDocument("$ifNull", new BsonArray
                                    {
                                        $"$$c.{DocumentFields.Conciliacion.Estado}",
                                        ""
                                    })),
                                    new BsonArray(ConciliacionEstados.FallidosOErrorComparacion.ToArray())
                                })
                            })
                        }
                    })
                }
            });
    }
}
