using Conciliaciones.Aplicacion.Features.Conciliation.Commands.UpdateConciliacionEstado;
using Conciliaciones.Aplicacion.Features.Conciliation.Querys.GetConciliationFuentesDetail;
using Conciliaciones.Aplicacion.Features.Conciliation.Querys.GetConciliationRows;
using Conciliaciones.Aplicacion.Features.Conciliation.Querys.GetConciliationsByIdSp;
using Conciliaciones.Aplicacion.Features.Process.Querys.GetProcessByIdProjects;
using Conciliaciones.Aplicacion.Features.Projects.Querys.GetResumenProyectos;
using Conciliaciones.Aplicacion.Features.SubProcess.Querys.GetReglasFallidasBySubproceso;
using Conciliaciones.Aplicacion.Features.SubProcess.Querys.GetSubProcessByIdProcess;
using Conciliaciones.Aplicacion.Ports.Interfaces;
using Conciliaciones.Dominio.DTOs.Response.Conciliation;
using Conciliaciones.Dominio.DTOs.Response.Process;
using Conciliaciones.Dominio.DTOs.Response.Projects;
using Conciliaciones.Dominio.DTOs.Response.SubProcess;
using Conciliaciones.Dominio.Ports.Interfaces.Process;
using Conciliaciones.Dominio.Ports.Interfaces.Proyects;
using Conciliaciones.Dominio.Ports.Interfaces.SubProcess;
using Conciliaciones.Pruebas.Support;
using FluentAssertions;
using Moq;
using NUnit.Framework;

namespace Conciliaciones.Pruebas.Conciliaciones.Aplicacion.NUnit
{
    [TestFixture]
    public class HandlerCancellationAndNullGapsTests : NUnitTestBase
    {
        [Test]
        public async Task GetConciliationRows_PassesCancellationToken()
        {
            Mock<IConciliationQueryService> service = FreezeMock<IConciliationQueryService>();
            using CancellationTokenSource cts = new();
            PagedRowsDto dto = new(0, 0, 0, 0, 1, 10, new List<Dictionary<string, object?>>());
            service.Setup(s => s.GetConciliationRowsAsync("r", 1, 10, cts.Token)).ReturnsAsync(dto);

            GetConciliationRowsHandler handler = new(service.Object);
            await handler.Handle(new GetConciliationRowsQuery("r", 1, 10), cts.Token);

            service.Verify(s => s.GetConciliationRowsAsync("r", 1, 10, cts.Token), Times.Once);
        }

        [Test]
        public async Task GetConciliationFuentesDetail_EmptyList_Ok()
        {
            Mock<IConciliationQueryService> service = FreezeMock<IConciliationQueryService>();
            service.Setup(s => s.GetConciliationFuentesDetailAsync("id", It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<FuenteDetalleDto>());

            GetConciliationFuentesDetailHandler handler = new(service.Object);
            var result = await handler.Handle(new GetConciliationFuentesDetailQuery("id"), CancellationToken.None);

            result.Resultado!.data.Should().BeEmpty();
        }

        [Test]
        public async Task UpdateEstado_EmptyIds_StillCallsService()
        {
            Mock<IConciliationStateService> service = FreezeMock<IConciliationStateService>();
            service.Setup(s => s.UpdateEstadoAsync(It.IsAny<List<Guid>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new StatusUpdateConciliation(new List<Guid>(), new List<Guid>(), 0));

            UpdateConciliacionEstadoHandler handler = new(service.Object);
            var result = await handler.Handle(
                new UpdateConciliacionEstadoCommand(new List<Guid>()),
                CancellationToken.None);

            result.Resultado.Should().NotBeNull();
            service.Verify(s => s.UpdateEstadoAsync(It.Is<List<Guid>>(l => l.Count == 0), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Test]
        public async Task GetResumenProyectos_EmptyCollection()
        {
            Mock<IProyectsReportRepository> repo = FreezeMock<IProyectsReportRepository>();
            repo.Setup(r => r.GetResumenProyectosAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<ProjectSummaryDto>());

            GetResumenProyectosHandler handler = new(repo.Object);
            var result = await handler.Handle(new GetResumenProyectosQuery(), CancellationToken.None);

            result.Resultado!.data.Should().BeEmpty();
        }

        [Test]
        public async Task GetProcessByIdProjects_EmptyCollection()
        {
            Mock<IProcessRepository> repo = FreezeMock<IProcessRepository>();
            repo.Setup(r => r.GetProcessesByIdProjectsAsync("p", It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<ProcesstSummaryDto>());

            GetProcessByIdProjectsHandler handler = new(repo.Object);
            var result = await handler.Handle(new GetProcessByIdProjectsQuery("p"), CancellationToken.None);

            result.Resultado!.data.Should().BeEmpty();
        }

        [Test]
        public async Task GetSubProcess_EmptyCollection()
        {
            Mock<ISubProcessRepository> repo = FreezeMock<ISubProcessRepository>();
            repo.Setup(r => r.GetSubProcesosByIdProcess("p", It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<SubProcessInfoDto>());

            GetSubProcessByIdProcessHandler handler = new(repo.Object);
            var result = await handler.Handle(new GetSubProcessByIdProcessQuery("p"), CancellationToken.None);

            result.Resultado!.data.Should().BeEmpty();
        }

        [Test]
        public async Task GetReglasFallidas_EmptyRules()
        {
            Mock<ISubProcessRepository> repo = FreezeMock<ISubProcessRepository>();
            repo.Setup(r => r.GetReglasFallidasBySubprocesoIdAsync("s", 1, 10, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new SubprocesoReglasResumenDto(0, 0, 1, 10, 0, new List<FailedRuleDto>()));

            GetReglasFallidasBySubprocesoHandler handler = new(repo.Object);
            var result = await handler.Handle(new GetReglasFallidasBySubprocesoQuery("s", 1, 10), CancellationToken.None);

            result.Resultado!.data.Should().BeEmpty();
        }

        [Test]
        public async Task GetConciliationsByIdSp_PassesToken()
        {
            Mock<IConciliationQueryService> service = FreezeMock<IConciliationQueryService>();
            using CancellationTokenSource cts = new();
            service.Setup(s => s.GetConciliationByIdSp("r", cts.Token))
                .ReturnsAsync(new ColumsDetailsDto(new List<BlockUiDto>()));

            GetConciliationsByIdSpHandler handler = new(service.Object);
            await handler.Handle(new GetConciliationsByIdSpQuery("r"), cts.Token);

            service.Verify(s => s.GetConciliationByIdSp("r", cts.Token), Times.Once);
        }
    }
}
