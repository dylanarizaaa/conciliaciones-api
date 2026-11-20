using Conciliaciones.Dominio.DTOs.Response.Conciliation;
using Conciliaciones.Infraestructura.Persistence.Documents;
using Conciliaciones.Infraestructura.Persistence.Documents.Projections;
using Conciliaciones.Infraestructura.Persistence.Services;
using MongoDB.Bson;

namespace Conciliaciones.Pruebas.Conciliaciones.Infraestructura.Persistence.Services
{
    [TestClass]
    public class ConciliationFuentesDetailComposerTests
    {
        private static ConciliationFuentesDetailProjection Proyeccion(Dictionary<string, object> data) =>
            new()
            {
                ConciliacionId = "c-1",
                NombreProceso = "Pago de Servicios",
                ResultadoFuentes =
                [
                    new ResultadoFuenteDocument
                    {
                        FuenteId = "fuentePrepagoMovimiento",
                        ConexionId = "ConexionPrepago",
                        Data = [data]
                    },
                    new ResultadoFuenteDocument { FuenteId = "fuenteControllerAdmision", ConexionId = "ConexionController" }
                ],
                UiConfigDoc = new ParametrizacionUiDocument
                {
                    Datos =
                    [
                        new UiDatosBlockDocument
                        {
                            IdFuente = "fuentePrepagoMovimiento",
                            Columns =
                            [
                                Columna("fuentePrepagoMovimiento.idMovimientoPrepago", "Id Movimiento Prepago", "number"),
                                Columna("valorFinal", "Valor Final", "currency"),
                                Columna("idDocumento", "Id Documento", "text"),
                                Columna("observacion", "Observación", "text")
                            ]
                        }
                    ]
                }
            };

        private static UiColumnDocument Columna(string field, string header, string tipo) =>
            new() { Field = field, Header = header, CellConfig = new UiCellConfigDocument { Type = tipo } };

        [TestMethod]
        public void Compose_EnviaElIdDeCadaFuenteYDeCadaDatoComoLosUsaLaPlantilla()
        {
            ConciliationFuentesDetailComposer composer = new();

            List<FuenteDetalleDto> fuentes = composer.Compose(Proyeccion(new Dictionary<string, object>
            {
                ["fuentePrepagoMovimiento.idMovimientoPrepago"] = 19957
            }));

            Assert.AreEqual(2, fuentes.Count);
            // Id de la fuente = idFuente (accordion.items[].id de la plantilla).
            Assert.AreEqual("fuentePrepagoMovimiento", fuentes[0].Id);
            Assert.AreEqual("Pago de Servicios ConexionPrepago", fuentes[0].Nombre);
            Assert.AreEqual("fuenteControllerAdmision", fuentes[1].Id);
            Assert.AreEqual(0, fuentes[1].Datos.Count);

            // Id del dato = alias del campo (accessorKey de la columna); Nombre = etiqueta.
            DatoDetalleDto dato = fuentes[0].Datos[0];
            Assert.AreEqual("idMovimientoPrepago", dato.Id);
            Assert.AreEqual("Id Movimiento Prepago", dato.Nombre);
            Assert.AreEqual("number", dato.Tipo);
            Assert.AreEqual(19957, dato.Valor);
        }

        [TestMethod]
        public void Compose_ConvierteLosTiposBsonEnValoresEscalares()
        {
            ConciliationFuentesDetailComposer composer = new();
            ObjectId documento = ObjectId.GenerateNewId();

            List<FuenteDetalleDto> fuentes = composer.Compose(Proyeccion(new Dictionary<string, object>
            {
                ["valorFinal"] = new Decimal128(6200000.50m),
                ["idDocumento"] = documento,
                ["observacion"] = BsonNull.Value
            }));

            List<DatoDetalleDto> datos = fuentes[0].Datos;
            // Sin convertir, System.Text.Json serializa Decimal128 como {}.
            Assert.AreEqual(6200000.50m, datos[1].Valor);
            Assert.AreEqual(documento.ToString(), datos[2].Valor);
            Assert.IsNull(datos[3].Valor);
            Assert.IsNull(datos[0].Valor);
        }
    }
}
