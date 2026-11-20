namespace Conciliaciones.Dominio.Utils
{
    public class ConstantData
    {
        public static DateTime FechaActual
        {
            get
            {
#if DEBUG
                return DateTime.Now;
#else
                return DateTime.Now.AddHours(-5);
#endif
            }
        }
    }
}
