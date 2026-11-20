using Conciliaciones.Dominio.Entities.Client;
using Conciliaciones.Dominio.Entities.Common;
using Conciliaciones.Dominio.Validators;
using Conciliaciones.Infraestructura.Persistence.Documents;

namespace Conciliaciones.Infraestructura.Persistence.Documents.Factories
{
    public static class HierarchyDocumentFactory
    {
        private static readonly ProyectoValidator ProyectoValidator = new();
        private static readonly ProcesoValidator ProcesoValidator = new();
        private static readonly SubProcesoValidator SubProcesoValidator = new();

        public static ProyectoDocument FromDomain(Proyecto entity)
        {
            EntityValidator.ValidateAndThrow(entity, ProyectoValidator);
            return new ProyectoDocument
            {
                IdProyecto = entity.IdProyecto,
                Nombre = entity.Nombre,
                FechaCreacion = EntityValidation.ToIso(entity.FechaCreacion)
            };
        }

        public static ProyectoDocument CreateProyecto(string id, string nombre) =>
            FromDomain(Proyecto.Create(nombre, id));

        public static ProcesoDocument FromDomain(Proceso entity)
        {
            EntityValidator.ValidateAndThrow(entity, ProcesoValidator);
            return new ProcesoDocument
            {
                IdProceso = entity.IdProceso,
                IdProyecto = entity.IdProyecto,
                Nombre = entity.Nombre,
                FechaCreacion = EntityValidation.ToIso(entity.FechaCreacion)
            };
        }

        public static ProcesoDocument CreateProceso(string id, string nombre, string idProyecto) =>
            FromDomain(Proceso.Create(nombre, idProyecto, id));

        public static SubProcesoDocument FromDomain(SubProceso entity)
        {
            EntityValidator.ValidateAndThrow(entity, SubProcesoValidator);
            return new SubProcesoDocument
            {
                IdSubProceso = entity.IdSubProceso,
                IdProceso = entity.IdProceso,
                Nombre = entity.Nombre,
                FechaCreacion = EntityValidation.ToIso(entity.FechaCreacion)
            };
        }

        public static SubProcesoDocument CreateSubProceso(string id, string nombre, string idProceso) =>
            FromDomain(SubProceso.Create(nombre, idProceso, id));

        public static Proyecto ToDomain(ProyectoDocument document) =>
            Proyecto.Rehydrate(
                document.IdProyecto,
                document.Nombre,
                EntityValidation.FromIsoOrUtcNow(document.FechaCreacion));

        public static Proceso ToDomain(ProcesoDocument document) =>
            Proceso.Rehydrate(
                document.IdProceso,
                document.IdProyecto,
                document.Nombre,
                EntityValidation.FromIsoOrUtcNow(document.FechaCreacion));

        public static SubProceso ToDomain(SubProcesoDocument document) =>
            SubProceso.Rehydrate(
                document.IdSubProceso,
                document.IdProceso,
                document.Nombre,
                EntityValidation.FromIsoOrUtcNow(document.FechaCreacion));
    }
}
