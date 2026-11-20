using Conciliaciones.Dominio.Entities.Client;
using Conciliaciones.Dominio.Validators;

namespace Conciliaciones.Pruebas.Conciliaciones.Dominio.Entities
{
    [TestClass]
    public class ProyectoEntityTests
    {
        [TestMethod]
        public void Create_ConNombreValido_AsignaIdYNombre()
        {
            Proyecto proyecto = Proyecto.Create("Recaudos");

            Assert.IsFalse(string.IsNullOrWhiteSpace(proyecto.IdProyecto));
            Assert.AreEqual("Recaudos", proyecto.Nombre);
            EntityValidator.ValidateAndThrow(proyecto, new ProyectoValidator());
        }

        [TestMethod]
        public void Create_ConNombreVacio_LanzaArgumentException()
        {
            try
            {
                _ = Proyecto.Create("  ");
                Assert.Fail("Se esperaba excepción.");
            }
            catch (ArgumentException)
            {
            }
        }
    }
}
