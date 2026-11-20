using Conciliaciones.Dominio.DTOs.Requests.Conciliation;
using Conciliaciones.Dominio.Entities.Parameterization;
using Conciliaciones.Dominio.Validators;
using MongoDB.Bson;

namespace Conciliaciones.Infraestructura.Persistence.Documents.Factories
{
    public static class ParametrizacionDocumentFactory
    {
        private static readonly ParameterizationEntityValidator Validator = new();

        public static ParametrizacionDocument FromDomain(ParameterizationEntity entity)
        {
            EntityValidator.ValidateAndThrow(entity, Validator);
            return new ParametrizacionDocument
            {
                ParametrizacionId = entity.ParametrizacionId,
                SubProcesoId = entity.SubProcesoId,
                FechaCreacion = entity.FechaCreacion,
                Parametros = entity.Parametros?.Select(p => new ParametroDocument
                {
                    Nombre = p.Nombre,
                    Tipo = p.Tipo
                }).ToList(),
                Fuentes = entity.Fuentes.Select(MapFuente).ToList(),
                Transformacion = entity.Transformacion.Select(MapTransformacion).ToList(),
                Reglas = entity.Reglas?.Select(MapRegla).ToList() ?? new List<ReglaItemDocument>()
            };
        }

        public static ParameterizationEntity ToDomain(ParametrizacionBodyDto? body, string parametrizacionId, string subProcesoId)
        {
            if (body == null)
            {
                return ParameterizationEntity.Create(subProcesoId, parametrizacionId);
            }

            return ParameterizationEntity.Create(
                subProcesoId,
                parametrizacionId,
                parametros: body.parametros?.Select(p => new ParametroEntity(p.nombre, p.tipo)),
                fuentes: body.fuentes?.Select(f => new FuenteEntity(
                    f.fuenteId,
                    f.conexionId,
                    f.tabla,
                    f.camposSeleccion?.Select(c => new CampoSeleccionEntity(c.nombre, c.tipoDato, c.alias)),
                    f.filtros?.Select(filtro => new FiltroEntity(
                        filtro.operador,
                        filtro.condiciones?.Select(cond => new CondicionFiltroEntity(cond.campo, cond.operador, cond.valor)))))),
                transformacion: body.transformacion?.Select(t => new TransformacionEntity(
                    t.alias,
                    t.tipo,
                    t.operacion,
                    t.operandos?.Select(o => new OperandoEntity(o.@ref)))),
                reglas: body.reglas?.Select(r => new ReglaEntity(
                    r.nombreRegla,
                    r.eventoExito,
                    r.mensajeError,
                    r.expresion,
                    r.idRegla)));
        }

        public static ParametrizacionDocument FromRequest(
            ParametrizacionBodyDto? body,
            string parametrizacionId,
            string subProcesoId) =>
            FromDomain(ToDomain(body, parametrizacionId, subProcesoId));

        private static FuenteParametrizacionDocument MapFuente(FuenteEntity f) =>
            new FuenteParametrizacionDocument
            {
                FuenteId = f.FuenteId,
                ConexionId = f.ConexionId,
                Tabla = f.Tabla,
                CamposSeleccion = f.CamposSeleccion?.Select(c => new CampoSeleccionDocument
                {
                    Nombre = c.Nombre,
                    TipoDato = c.TipoDato,
                    Alias = c.Alias
                }).ToList(),
                Filtros = f.Filtros?.Select(filtro => new FiltroDocument
                {
                    Operador = filtro.Operador,
                    Condiciones = filtro.Condiciones?.Select(cond => new CondicionFiltroDocument
                    {
                        Campo = cond.Campo,
                        Operador = cond.Operador,
                        Valor = cond.Valor != null ? BsonValue.Create(ResolveJsonValue(cond.Valor)) : BsonNull.Value
                    }).ToList()
                }).ToList()
            };

        private static TransformacionDocument MapTransformacion(TransformacionEntity t) =>
            new TransformacionDocument
            {
                Alias = t.Alias,
                Tipo = t.Tipo,
                Operacion = t.Operacion,
                Operandos = t.Operandos.Select(o => new OperandoDocument { Ref = o.Ref }).ToList()
            };

        private static ReglaItemDocument MapRegla(ReglaEntity regla) =>
            new ReglaItemDocument
            {
                IdRegla = regla.IdRegla,
                NombreRegla = regla.NombreRegla,
                EventoExito = regla.EventoExito,
                MensajeError = regla.MensajeError,
                Expresion = regla.Expresion
            };

        private static object? ResolveJsonValue(object? valor)
        {
            if (valor is System.Text.Json.JsonElement element)
            {
                return element.ValueKind switch
                {
                    System.Text.Json.JsonValueKind.String => element.GetString(),
                    System.Text.Json.JsonValueKind.Number => element.TryGetInt64(out long l) ? l : element.GetDouble(),
                    System.Text.Json.JsonValueKind.True => true,
                    System.Text.Json.JsonValueKind.False => false,
                    System.Text.Json.JsonValueKind.Array => element.EnumerateArray().Select(e => ResolveJsonValue(e)).ToList(),
                    System.Text.Json.JsonValueKind.Null or System.Text.Json.JsonValueKind.Undefined => null,
                    _ => element.GetRawText()
                };
            }

            return valor;
        }
    }
}
