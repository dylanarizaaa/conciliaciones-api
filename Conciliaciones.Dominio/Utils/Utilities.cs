using System.Diagnostics.CodeAnalysis;

namespace Conciliaciones.Dominio.Utils
{
    [ExcludeFromCodeCoverage]

    public class Utilities
    {
        /// <summary>
        /// Metodo para obtener las variables de entorno
        /// </summary>
        /// <param name="nombreVariable"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        /// 
        public static string ObtenerVariableEntorno(string nombreVariable)
        {
            try
            {
                string? valor = Environment.GetEnvironmentVariable(nombreVariable);
                if (string.IsNullOrEmpty(valor))
                {
                    throw new Exception($"La variable de entorno '{nombreVariable}' no está configurada.");
                }
                return valor;

            }
            catch (Exception ex)
            {
                throw new Exception($"Error al obtener la variable de entorno '{nombreVariable}': {ex.Message}");
            }

        }
    }
}
