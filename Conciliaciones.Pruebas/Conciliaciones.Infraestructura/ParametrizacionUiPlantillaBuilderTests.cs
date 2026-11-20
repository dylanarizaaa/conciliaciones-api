using Conciliaciones.Dominio.DTOs.Requests.Conciliation;
using Conciliaciones.Infraestructura.Persistence.Documents.Factories;
using MongoDB.Bson;

namespace Conciliaciones.Pruebas.Conciliaciones.Infraestructura.Persistence.Documents.Factories
{
    [TestClass]
    public class ParametrizacionUiPlantillaBuilderTests
    {
        [TestMethod]
        public void Build_ConFuente_ArmaTablaConCheckboxEstadoYAccion()
        {
            parametrizacionUiBodyDto ui = new()
            {
                fuente =
                [
                    new fuenteUiColumnaInputDto
                    {
                        fuenteDato = "fuentePrepagoDetalleCliente.nombreCompleto",
                        tipo = "text",
                        label = "Nombre del cliente"
                    },
                    new fuenteUiColumnaInputDto
                    {
                        fuenteDato = "fechaTransaccion",
                        tipo = "date",
                        label = "Fecha de transacción"
                    },
                    new fuenteUiColumnaInputDto
                    {
                        fuenteDato = "montoInterPay",
                        tipo = "number",
                        label = "Monto Inter Pay"
                    }
                ]
            };

            BsonDocument? plantilla = ParametrizacionUiPlantillaBuilder.Build(ui);

            Assert.IsNotNull(plantilla);
            BsonDocument table = plantilla["table"].AsBsonDocument;
            BsonDocument block = table["blocks"][0].AsBsonDocument;
            Assert.AreEqual("table", block["type"].AsString);
            Assert.AreEqual(block["id"].AsString, table["layout"]["items"][0]["blockId"].AsString);

            BsonArray columns = block["table"]["columns"].AsBsonArray;
            Assert.AreEqual("seleccion", columns[0]["id"].AsString);
            Assert.AreEqual("nombreCompleto", columns[1]["id"].AsString);
            Assert.AreEqual("nombreCompleto", columns[1]["accessorKey"].AsString);
            Assert.AreEqual("TextCell", columns[1]["cell"]["component"].AsString);
            Assert.AreEqual("lead", columns[1]["cell"]["inputs"]["emphasis"].AsString);
            Assert.AreEqual("DateCell", columns[2]["cell"]["component"].AsString);
            Assert.AreEqual("CurrencyCell", columns[3]["cell"]["component"].AsString);
            Assert.AreEqual("estado", columns[4]["id"].AsString);
            Assert.AreEqual("accion", columns[5]["id"].AsString);
            Assert.AreEqual("/Conciliaciones/subprocesos", table["blocks"][0]["dataSource"]["path"].AsString);
        }

        [TestMethod]
        public void Build_ConDatos_TituloConcatenaLabelYFuenteDato()
        {
            parametrizacionUiBodyDto ui = new()
            {
                datos =
                [
                    new fuenteUiDatosInputDto
                    {
                        idFuente = "alcarrito",
                        datos =
                        [
                            new fuenteUiColumnaInputDto
                            {
                                fuenteDato = "alcarrito.numeroGuia",
                                tipo = "text",
                                label = "Número de guía"
                            },
                            new fuenteUiColumnaInputDto
                            {
                                fuenteDato = "alcarrito.recaudo",
                                tipo = "number",
                                label = "Recaudo"
                            }
                        ]
                    }
                ]
            };

            BsonDocument? plantilla = ParametrizacionUiPlantillaBuilder.Build(ui);

            Assert.IsNotNull(plantilla);
            BsonDocument accordion = plantilla["accordion"].AsBsonDocument;
            BsonDocument item = accordion["blocks"][0]["accordion"]["items"][0].AsBsonDocument;
            Assert.AreEqual("Número de guía: {numeroGuia}", item["content"]["title"].AsString);
            Assert.AreEqual("numeroGuia", item["content"]["columns"][0]["accessorKey"].AsString);
            Assert.AreEqual("recaudo", item["content"]["columns"][1]["accessorKey"].AsString);
            Assert.AreEqual("CurrencyCell", item["content"]["columns"][1]["cell"]["component"].AsString);
        }

        [TestMethod]
        public void Build_SinFuenteNiDatos_RetornaNull()
        {
            Assert.IsNull(ParametrizacionUiPlantillaBuilder.Build(new parametrizacionUiBodyDto()));
            Assert.IsNull(ParametrizacionUiPlantillaBuilder.Build(null));
        }
    }
}
