using Conciliaciones.Aplicacion.Features.Conciliation.Commands.CreateParametrizacionFull;
using Conciliaciones.Aplicacion.Features.Conciliation.Querys.GetConciliationRows;
using FluentAssertions;
using NUnit.Framework;

namespace Conciliaciones.Pruebas.Conciliaciones.Aplicacion.NUnit
{
    [TestFixture]
    public class ApplicationDtoCoverageTests
    {
        [Test]
        public void CreateParametrizacionFullResponse_ExposesAllProperties()
        {
            CreateParametrizacionFullResponse response = new(
                "proy",
                "proc",
                "sub",
                "param",
                "ok");

            response.IdProyecto.Should().Be("proy");
            response.IdProceso.Should().Be("proc");
            response.IdSubProceso.Should().Be("sub");
            response.ParametrizacionId.Should().Be("param");
            response.Mensaje.Should().Be("ok");
        }

        [Test]
        public void GetConciliationRowsResponse_ExposesTotalCount()
        {
            GetConciliationRowsResponse response = new(
                casosTotales: 1,
                montoAfectado: 10m,
                totalCount: 5,
                totalPages: 1,
                pageNumber: 1,
                pageSize: 10,
                data: new List<Dictionary<string, object?>>());

            response.totalCount.Should().Be(5);
            response.pageSize.Should().Be(10);
            response.data.Should().BeEmpty();
        }
    }
}