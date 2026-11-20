using Conciliaciones.Dominio.Entities.Common;
using Conciliaciones.Infraestructura.Persistence.Documents.Factories;
using MongoDB.Bson;

namespace Conciliaciones.Pruebas.Conciliaciones.Infraestructura.Persistence.Documents.Factories
{
    [TestClass]
    public class PlantillaEstadoColumnaTests
    {
        private static readonly string[] EstadosReales =
        [
            ConciliacionEstados.con_novedad,
            ConciliacionEstados.error_evaluacion,
            ConciliacionEstados.datos_incompletos,
            ConciliacionEstados.validando,
            ConciliacionEstados.conciliado
        ];

        [TestMethod]
        public void Build_LeeElCampoDeLasFilasYTieneUnaVariantePorEstadoReal()
        {
            BsonDocument columna = PlantillaEstadoColumna.Build();

            Assert.AreEqual("estado", columna["id"].AsString);
            Assert.AreEqual("estadoConciliacion", columna["accessorKey"].AsString);
            Assert.AreEqual("BadgeCell", columna["cell"]["component"].AsString);
            BsonDocument variants = columna["cell"]["inputs"]["variants"].AsBsonDocument;
            CollectionAssert.AreEquivalent(EstadosReales, variants.Names.ToArray());
            Assert.AreEqual("Sin estado", columna["cell"]["inputs"]["fallback"]["label"].AsString);
        }

        [TestMethod]
        public void Aplicar_CorrigeLaColumnaDeEstadoDeUnaPlantillaYaGuardada()
        {
            BsonDocument tabla = BsonDocument.Parse("""
                {
                  "layout": { "type": "stack", "items": [ { "blockId": "b1" } ] },
                  "blocks": [ {
                    "id": "b1", "type": "table",
                    "table": { "rowId": "conciliacionId", "columns": [
                      { "id": "primerNombre", "accessorKey": "primerNombre", "header": "Primer Nombre" },
                      { "id": "estado", "accessorKey": "estado", "header": { "component": "SortIndicator", "inputs": { "label": "Estado" } },
                        "cell": { "component": "BadgeCell", "inputs": { "variants": { "Fallido": { "label": "Fallido", "tone": "danger" } } } },
                        "meta": { "width": 206 }, "pin": "end" }
                    ] }
                  } ]
                }
                """);

            PlantillaEstadoColumna.Aplicar(tabla);

            BsonArray columnas = tabla["blocks"][0]["table"]["columns"].AsBsonArray;
            BsonDocument estado = columnas[1].AsBsonDocument;
            Assert.AreEqual("estadoConciliacion", estado["accessorKey"].AsString);
            CollectionAssert.AreEquivalent(EstadosReales, estado["cell"]["inputs"]["variants"].AsBsonDocument.Names.ToArray());
            // El resto de la columna y de la tabla queda como estaba.
            Assert.AreEqual(206, estado["meta"]["width"].AsInt32);
            Assert.AreEqual("end", estado["pin"].AsString);
            Assert.AreEqual("primerNombre", columnas[0]["accessorKey"].AsString);
        }

        [TestMethod]
        public void Aplicar_NoFallaConUnaTablaSinBloquesNiColumnas()
        {
            BsonDocument sinBloques = new("layout", new BsonDocument());
            BsonDocument sinColumnas = BsonDocument.Parse("""{ "blocks": [ { "id": "b1" }, 3 ] }""");

            PlantillaEstadoColumna.Aplicar(sinBloques);
            PlantillaEstadoColumna.Aplicar(sinColumnas);

            Assert.IsFalse(sinBloques.Contains("blocks"));
        }
    }
}
