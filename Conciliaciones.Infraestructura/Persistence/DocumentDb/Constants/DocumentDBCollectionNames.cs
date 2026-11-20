using System.Diagnostics.CodeAnalysis;


namespace Conciliaciones.Infraestructura.Persistence.DocumentDb.Constants
{

    [ExcludeFromCodeCoverage]
    public class DocumentDBCollectionNames
    {
        /// <summary>
        /// Nombres de las colecciones en MongoDB.
        /// </summary>
        public static class MongoCollectionNames
        {
            public const string Conciliaciones = "conciliaciones";
            public const string Parametrizacion = "parametrizacion";
            public const string Proyectos = "proyectos";
            public const string Procesos = "procesos";
            public const string SubProcesos = "subprocesos";
            public const string ParametrizacionUI = "parametrizacionUI";
            public const string Automatizaciones = "automatizaciones";
            public const string Eventos = "eventos";

            public const string HistorialConciliaciones = "historialConciliaciones";
        }

    }
}
