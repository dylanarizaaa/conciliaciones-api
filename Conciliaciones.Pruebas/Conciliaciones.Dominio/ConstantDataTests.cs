using Conciliaciones.Dominio.Utils;

namespace Conciliaciones.Pruebas.Conciliaciones.Dominio.Utils
{
    [TestClass]
    public class ConstantDataTests
    {
        [TestMethod]
        public void FechaActual_AlConsultar_RetornaFechaValidaYReciente()
        {
            // Arrange
            DateTime fechaInicioPrueba = DateTime.Now.AddMinutes(-1);

            // Act
            DateTime fechaObtenida = ConstantData.FechaActual;

            // Assert
            Assert.AreNotEqual(default(DateTime), fechaObtenida);
            Assert.IsTrue(fechaObtenida >= fechaInicioPrueba.AddHours(-6)); // Tolerancia por diferencia de zona horaria / compilación
            Assert.IsTrue(fechaObtenida <= DateTime.Now.AddMinutes(1));
        }

        [TestMethod]
        public void FechaActual_ConsultasConsecutivas_RetornaValoresConsistentes()
        {
            // Arrange & Act
            DateTime primeraConsulta = ConstantData.FechaActual;
            DateTime segundaConsulta = ConstantData.FechaActual;

            // Assert
            Assert.IsTrue(segundaConsulta >= primeraConsulta);
        }
    }
}