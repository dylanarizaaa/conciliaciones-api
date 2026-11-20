using Conciliaciones.Dominio.Entities.Client;
using Conciliaciones.Dominio.Entities.Common;
using Conciliaciones.Dominio.Entities.Conciliation;
using Conciliaciones.Dominio.Entities.Parameterization;
using Conciliaciones.Dominio.Validators;
using FluentAssertions;
using NUnit.Framework;
using DomainValidationException = Conciliaciones.Dominio.Exceptions.ValidationException;

namespace Conciliaciones.Pruebas.Conciliaciones.Dominio.NUnit
{
    [TestFixture]
    public class EntityValidationAndEntitiesTests
    {
        [Test]
        public void EntityValidation_Required_Null_Throws()
        {
            Action act = () => EntityValidation.Required(null, "campo");
            act.Should().Throw<ArgumentException>();
        }

        [Test]
        public void EntityValidation_FromIsoOrUtcNow_Invalid_UsesUtcNow()
        {
            DateTime before = DateTime.UtcNow.AddSeconds(-2);
            DateTime result = EntityValidation.FromIsoOrUtcNow("not-iso");
            result.Should().BeOnOrAfter(before);
        }

        [Test]
        public void Proceso_Create_HappyPath_AndValidator()
        {
            Proceso proceso = Proceso.Create("Proc", Guid.NewGuid().ToString());
            EntityValidator.ValidateAndThrow(proceso, new ProcesoValidator());
            proceso.Nombre.Should().Be("Proc");
        }

        [Test]
        public void Proceso_Create_EmptyParent_Throws()
        {
            Action act = () => Proceso.Create("Proc", " ");
            act.Should().Throw<ArgumentException>();
        }

        [Test]
        public void SubProceso_Create_HappyPath()
        {
            SubProceso sub = SubProceso.Create("Sub", Guid.NewGuid().ToString());
            EntityValidator.ValidateAndThrow(sub, new SubProcesoValidator());
            sub.IdSubProceso.Should().NotBeNullOrWhiteSpace();
        }

        [Test]
        public void ParameterizationEntity_Create_EmptyFuentes_Ok()
        {
            ParameterizationEntity entity = ParameterizationEntity.Create(Guid.NewGuid().ToString());
            entity.Fuentes.Should().BeEmpty();
            entity.Transformacion.Should().BeEmpty();
            EntityValidator.ValidateAndThrow(entity, new ParameterizationEntityValidator());
        }

        [Test]
        public void ParameterizationEntity_WithReglasSinNombre_FailsValidation()
        {
            ParameterizationEntity entity = ParameterizationEntity.Create(
                Guid.NewGuid().ToString(),
                reglas: new[] { new ReglaEntity("", "ok", "err", "1==1") });

            Action act = () => EntityValidator.ValidateAndThrow(entity, new ParameterizationEntityValidator());
            act.Should().Throw<DomainValidationException>();
        }

        [Test]
        public void ParameterizationUIEntity_Create_AndValidate()
        {
            ParameterizationUIEntity ui = ParameterizationUIEntity.Create(
                Guid.NewGuid().ToString(),
                new[] { new UiColumnEntity("f1", "H1", 1, cellConfig: UiCellConfigEntity.CurrencyCop()) },
                Array.Empty<UiDatosBlockEntity>());

            EntityValidator.ValidateAndThrow(ui, new ParameterizationUIEntityValidator());
            ui.Fuente.Should().HaveCount(1);
            ui.Datos.Should().BeEmpty();
        }

        [Test]
        public void UiCellConfigEntity_FromType_Branches()
        {
            UiCellConfigEntity.FromType("currency").Type.Should().Be("currency");
            UiCellConfigEntity.FromType("date").Format.Should().Be("YYYY-MM-DD");
            UiCellConfigEntity.FromType(null).Type.Should().Be("text");
            UiCellConfigEntity.SelectRow().Type.Should().Be("select-row");
        }

        [Test]
        public void ConciliacionEntity_EsFallidaOError_Branches()
        {
            ConciliacionEntity fallida = ConciliacionEntity.Rehydrate(
                Guid.NewGuid().ToString(), null, ConciliacionEstados.con_novedad, null, null, null);
            ConciliacionEntity ok = ConciliacionEntity.Rehydrate(
                Guid.NewGuid().ToString(), null, "exitoso", null, Array.Empty<ResultadoFuenteEntity>(), null);

            fallida.EsFallidaOError.Should().BeTrue();
            ok.EsFallidaOError.Should().BeFalse();
            ok.ResultadoFuentes.Should().BeEmpty();
        }

        [Test]
        public void ConciliacionEntity_Rehydrate_EmptyId_Throws()
        {
            Action act = () => ConciliacionEntity.Rehydrate(" ", null, null, null, null, null);
            act.Should().Throw<ArgumentException>();
        }

        [Test]
        public void EntityValidator_Invalid_ThrowsDomainValidationException()
        {
            Proyecto bad = Proyecto.Rehydrate("id", "ok", DateTime.UtcNow);
            // Force invalid via validator with empty name through Rehydrate already requires name.
            // Use FluentValidation directly on empty-like scenario via custom invalid entity recreation:
            Action act = () => EntityValidator.ValidateAndThrow(
                Proyecto.Create(new string('x', 300)),
                new ProyectoValidator());
            act.Should().Throw<DomainValidationException>();
        }
    }
}
