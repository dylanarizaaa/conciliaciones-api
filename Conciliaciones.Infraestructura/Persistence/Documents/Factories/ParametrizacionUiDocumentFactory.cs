using Conciliaciones.Dominio.DTOs.Requests.Conciliation;
using Conciliaciones.Dominio.Entities.Common;
using Conciliaciones.Dominio.Entities.Parameterization;
using Conciliaciones.Dominio.Validators;
using Conciliaciones.Infraestructura.Persistence.DocumentDb.Constants;

namespace Conciliaciones.Infraestructura.Persistence.Documents.Factories
{
    public static class ParametrizacionUiDocumentFactory
    {
        private static readonly ParameterizationUIEntityValidator Validator = new();

        public static ParametrizacionUiDocument FromDomain(ParameterizationUIEntity entity)
        {
            EntityValidator.ValidateAndThrow(entity, Validator);
            return new ParametrizacionUiDocument
            {
                IdParametrizacion = entity.IdParametrizacion,
                Fuente = entity.Fuente.Select(MapColumn).ToList(),
                Datos = entity.Datos.Select(block => new UiDatosBlockDocument
                {
                    IdFuente = block.IdFuente,
                    Columns = block.Columns.Select(MapColumn).ToList()
                }).ToList()
            };
        }

        public static ParameterizationUIEntity ToDomain(string parametrizacionId, parametrizacionUiBodyDto? uiInput)
        {
            int order = 1;
            List<UiColumnEntity> fuenteCols = new List<UiColumnEntity>();

            if (uiInput?.fuente != null)
            {
                foreach (fuenteUiColumnaInputDto col in uiInput.fuente)
                {
                    fuenteCols.Add(new UiColumnEntity(
                        col.fuenteDato,
                        col.label,
                        order++,
                        cellConfig: UiCellConfigEntity.FromType(col.tipo)));
                }
            }

            fuenteCols.Add(new UiColumnEntity(
                "estado",
                "Estado",
                order++,
                cellConfig: UiCellConfigEntity.Badge(new Dictionary<string, string>
                {
                    { ConciliacionEstados.con_novedad, "destructive" },
                    { ConciliacionEstados.validando, "outline" },
                    { ConciliacionEstados.error_evaluacion, "outline" },
                    { ConciliacionEstados.datos_incompletos, "secondary" }
                })));

            fuenteCols.Add(new UiColumnEntity(
                "accion",
                "Acción",
                order++,
                cellConfig: UiCellConfigEntity.SelectRow()));

            List<UiDatosBlockEntity> datos = new List<UiDatosBlockEntity>();
            if (uiInput?.datos != null)
            {
                foreach (fuenteUiDatosInputDto block in uiInput.datos)
                {
                    int subOrder = 1;
                    List<UiColumnEntity> cols = block.datos
                        .Select(c => new UiColumnEntity(c.fuenteDato, c.label, subOrder++, cellConfig: UiCellConfigEntity.FromType(c.tipo)))
                        .ToList();
                    datos.Add(new UiDatosBlockEntity(block.idFuente, cols));
                }
            }

            return ParameterizationUIEntity.Create(parametrizacionId, fuenteCols, datos);
        }

        public static ParametrizacionUiDocument FromRequest(string parametrizacionId, parametrizacionUiBodyDto? uiInput)
        {
            ParametrizacionUiDocument document = FromDomain(ToDomain(parametrizacionId, uiInput));
            document.Plantilla = ParametrizacionUiPlantillaBuilder.Build(uiInput);
            return document;
        }

        private static UiColumnDocument MapColumn(UiColumnEntity column) =>
            new UiColumnDocument
            {
                Field = column.Field,
                Header = column.Header,
                Sortable = column.Sortable,
                Visible = column.Visible,
                Order = column.Order,
                CellConfig = MapCellConfig(column.CellConfig)
            };

        private static UiCellConfigDocument MapCellConfig(UiCellConfigEntity config)
        {
            UiCellConfigDocument document = new UiCellConfigDocument
            {
                Type = config.Type,
                Format = config.Format,
                VariantByValue = config.VariantByValue?.ToDictionary(kv => kv.Key, kv => kv.Value)
            };

            if (config.Currency != null)
            {
                document.Config = new Dictionary<string, object>
                {
                    { DocumentFields.CurrencyConfig.CurrencyCode, config.Currency.CurrencyCode },
                    { DocumentFields.CurrencyConfig.Display, config.Currency.Display },
                    { DocumentFields.CurrencyConfig.DigitsInfo, config.Currency.DigitsInfo },
                    { DocumentFields.CurrencyConfig.Locale, config.Currency.Locale },
                    { DocumentFields.CurrencyConfig.Symbol, config.Currency.Symbol }
                };
            }

            return document;
        }
    }
}
