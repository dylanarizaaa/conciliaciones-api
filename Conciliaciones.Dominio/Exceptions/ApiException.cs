using System.Globalization;

namespace Conciliaciones.Dominio.Exceptions
{

    public class ApiException : Exception
    {
        public ApiException() : base() { }
        public ApiException(string message) : base(message) { }
        public ApiException(string message, params object[] innerException) : base(String.Format(CultureInfo.CurrentCulture, message, innerException)) { }

    }

}
