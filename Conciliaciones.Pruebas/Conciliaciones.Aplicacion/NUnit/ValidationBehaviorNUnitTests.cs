using Conciliaciones.Aplicacion.Common.Behaviors;
using Conciliaciones.Pruebas.Support;
using FluentAssertions;
using FluentValidation;
using FluentValidation.Results;
using MediatR;
using Moq;
using NUnit.Framework;
using DomainValidationException = Conciliaciones.Dominio.Exceptions.ValidationException;

namespace Conciliaciones.Pruebas.Conciliaciones.Aplicacion.NUnit
{
    [TestFixture]
    public class ValidationBehaviorNUnitTests : NUnitTestBase
    {
        public sealed class Req : IRequest<string>
        {
            public string Name { get; set; } = string.Empty;
        }

        [Test]
        public async Task Handle_CancelledToken_Throws()
        {
            ValidationBehavior<Req, string> behavior = new(Array.Empty<IValidator<Req>>());
            using CancellationTokenSource cts = new();
            cts.Cancel();

            Func<Task> act = async () => await behavior.Handle(
                new Req(),
                _ => Task.FromResult("ok"),
                cts.Token);

            await act.Should().ThrowAsync<OperationCanceledException>();
        }

        [Test]
        public async Task Handle_Failures_ThrowsDomainValidationException_NotFluent()
        {
            Mock<IValidator<Req>> validator = new();
            validator.Setup(v => v.ValidateAsync(It.IsAny<ValidationContext<Req>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ValidationResult(new[] { new ValidationFailure("Name", "required") }));

            ValidationBehavior<Req, string> behavior = new(new[] { validator.Object });
            bool nextCalled = false;

            Func<Task> act = async () => await behavior.Handle(
                new Req(),
                _ =>
                {
                    nextCalled = true;
                    return Task.FromResult("ok");
                },
                CancellationToken.None);

            DomainValidationException ex =
                (await act.Should().ThrowAsync<DomainValidationException>()).Which;
            ex.Errors.Should().ContainSingle().Which.Should().Be("required");
            nextCalled.Should().BeFalse();
        }

        [Test]
        public async Task Handle_EmptyFailures_CallsNext()
        {
            Mock<IValidator<Req>> validator = new();
            validator.Setup(v => v.ValidateAsync(It.IsAny<ValidationContext<Req>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ValidationResult());

            ValidationBehavior<Req, string> behavior = new(new[] { validator.Object });

            string result = await behavior.Handle(new Req { Name = "x" }, _ => Task.FromResult("ok"), CancellationToken.None);

            result.Should().Be("ok");
        }
    }
}
