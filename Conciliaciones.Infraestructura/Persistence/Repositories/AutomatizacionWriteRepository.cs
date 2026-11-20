using Conciliaciones.Dominio.DTOs.Requests.Automatizaciones;
using Conciliaciones.Dominio.DTOs.Response.Automatizaciones;
using Conciliaciones.Dominio.Ports.Interfaces.Automatizaciones;
using Conciliaciones.Infraestructura.Persistence.DocumentDb.Constants;
using Conciliaciones.Infraestructura.Persistence.Documents;
using Conciliaciones.Infraestructura.Ports.Interfaces.DBContext;
using MongoDB.Driver;
using System.Net;

namespace Conciliaciones.Infraestructura.Persistence.Repositories
{
    public sealed class AutomatizacionWriteRepository : IAutomatizacionWritePort
    {
        private readonly IDocumentDBContext _dbContext;

        public AutomatizacionWriteRepository(IDocumentDBContext dbContext)
        {
            _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        }

        public async Task<(HttpStatusCode Status, CreateAutomatizacionResponse? Data, string Mensaje)> CreateAsync(
            CreateAutomatizacionRequest request,
            CancellationToken cancellationToken)
        {
            if (request == null)
            {
                throw new ArgumentNullException(nameof(request));
            }

            string? parametrizacionId = request.ParametrizacionId?.Trim();
            if (string.IsNullOrWhiteSpace(parametrizacionId))
            {
                return (HttpStatusCode.BadRequest, null, "parametrizacionId es obligatorio.");
            }

            IMongoCollection<ParametrizacionDocument> paramCol =
                _dbContext.GetCollection<ParametrizacionDocument>(
                    DocumentDBCollectionNames.MongoCollectionNames.Parametrizacion);

            bool paramExists = await paramCol
                .Find(Builders<ParametrizacionDocument>.Filter.Eq(x => x.ParametrizacionId, parametrizacionId))
                .AnyAsync(cancellationToken);

            if (!paramExists)
            {
                return (HttpStatusCode.NotFound, null, "La parametrización indicada no existe.");
            }

            IMongoCollection<AutomatizacionDocument> autoCol =
                _dbContext.GetCollection<AutomatizacionDocument>(
                    DocumentDBCollectionNames.MongoCollectionNames.Automatizaciones);

            bool alreadyRegistered = await autoCol
                .Find(Builders<AutomatizacionDocument>.Filter.Eq(x => x.ParametrizacionId, parametrizacionId))
                .AnyAsync(cancellationToken);

            if (alreadyRegistered)
            {
                return (HttpStatusCode.Conflict, null, "Ya existe una automatización para esa parametrización.");
            }

            string automatizacionId = Guid.NewGuid().ToString();


            AutomatizacionDocument document = MapDocument(request, automatizacionId, parametrizacionId);
            try
            {
                await autoCol.InsertOneAsync(document, cancellationToken: cancellationToken);
            } catch (MongoWriteException ex) when (ex.WriteError?.Category == ServerErrorCategory.DuplicateKey)
            {
                return (HttpStatusCode.Conflict, null, "Ya existe una automatización para esa parametrización.");
            }

            return (
                HttpStatusCode.OK,
                new CreateAutomatizacionResponse(
                    automatizacionId,
                    parametrizacionId,
                    "Automatización creada correctamente."),
                "Automatización creada correctamente.");
        }

        private static AutomatizacionDocument MapDocument(
            CreateAutomatizacionRequest request,
            string automatizacionId,
            string parametrizacionId) =>
            new()
            {
                AutomatizacionId = automatizacionId,
                ParametrizacionId = parametrizacionId,
                Estado = string.IsNullOrWhiteSpace(request.Estado) ? "activo" : request.Estado.Trim(),
                FechaCreacion = DateTime.UtcNow.ToString("o"),
                //Ajustar el dato de la fecha para el tipo Time 
                FechaFinal = request.FechaFinal,
                Reintento = request.Reintento,
                Intentos = request.Intentos,
                EsperaReintento = request.EsperaReintento,
                PreFuentes = request.PreFuentes?.Select(MapFuente).ToList(),
                MarcaAgua = request.MarcaAgua == null
                    ? null
                    : new MarcaAguaDocument
                    {
                        IndiceFinal = request.MarcaAgua.IndiceFinal,
                        FechaBusqueda = request.MarcaAgua.FechaBusqueda
                    },
                Intervalo = request.Intervalo == null
                    ? null
                    : new IntervaloDocument
                    {
                        Valor = request.Intervalo.Valor,
                        Unidad = request.Intervalo.Unidad
                    }
            };

        private static FuenteParametrizacionDocument MapFuente(AutomatizacionFuenteDto fuente) =>
            new()
            {
                FuenteId = fuente.FuenteId,
                ConexionId = fuente.ConexionId,
                Tabla = fuente.Tabla,
                CamposSeleccion = fuente.CamposSeleccion?.Select(c => new CampoSeleccionDocument
                {
                    Nombre = c.Nombre,
                    TipoDato = c.TipoDato,
                    Alias = c.Alias
                }).ToList(),
                Filtros = fuente.Filtros?.Select(f => new FiltroDocument
                {
                    Operador = f.Operador,
                    Condiciones = f.Condiciones?.Select(cond => new CondicionFiltroDocument
                    {
                        Campo = cond.Campo,
                        Operador = cond.Operador,
                        Valor = cond.Valor
                    }).ToList()
                }).ToList()
            };
    }
}
