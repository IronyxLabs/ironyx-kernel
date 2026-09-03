using AutoBogus;
using FluentValidation;
using FluentValidation.Results;
using Google.Rpc;
using Grpc.Core;
using Ironyx.Kernel.Execution.Dispatchers;
using Ironyx.Kernel.Execution.Exceptions;
using Ironyx.Kernel.Extractors;
using Ironyx.Kernel.Options;
using Ironyx.Kernel.Receivers;
using Ironyx.Kernel.Serializers;
using Ironyx.Kernel.Test.Unit.Kernel.Fakers;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Moq.Language.Flow;
using System.Text.Json;
using Xunit.Abstractions;

namespace Ironyx.Kernel.Test.Unit.Kernel.Endpoints
{
    public class GrpcEndpointTest
    {

        private readonly ILogger<GrpcEndpoint> _logger;

        private Mock<IRequestDeserializer> _deserializerMock = null!;
        private Mock<IExtractor> _extractorMock = null!;
        private Mock<IRequestContextAccessor> _requestContextMock = null!;
        private Mock<ICommandDispatcher> _commandDispatcherMock = null!;
        private Mock<IQueryDispatcher> _queryDispatcherMock = null!;
        private Mock<IOptionsMonitor<ServiceOptions>> _optionsMock = null!;

        public GrpcEndpointTest(ITestOutputHelper outputHelper)
        {
            _logger = new LoggerFactory()
                          .AddXUnit(outputHelper)
                          .CreateLogger<GrpcEndpoint>();
        }

        private GrpcEndpoint CreateSUT()
        {
            _deserializerMock = new Mock<IRequestDeserializer>();
            _extractorMock = new Mock<IExtractor>();
            _requestContextMock = new Mock<IRequestContextAccessor>();
            _commandDispatcherMock = new Mock<ICommandDispatcher>();
            _queryDispatcherMock = new Mock<IQueryDispatcher>();
            _optionsMock = new Mock<IOptionsMonitor<ServiceOptions>>();

            return new GrpcEndpoint(_deserializerMock.Object, _extractorMock.Object, _requestContextMock.Object, _commandDispatcherMock.Object, _queryDispatcherMock.Object, _optionsMock.Object, _logger);
        }

        [Fact(DisplayName = "[UNIT][GRE-001]: Receiving Command")]
        [GrpcEndpointFeature]
        public async Task GrpcEndpoint_SendAsync_ReceivingCommand()
        {
            // Arrange
            var sut = CreateSUT();
            var command = new AutoFaker<TestCommand>().Generate();

            _deserializerMock.Setup(d => d.DeserializeAsync(It.IsAny<Envelop>(), It.IsAny<CancellationToken>())).ReturnsAsync(command);

            // Act
            await sut.SendAsync(new EnvelopFaker().Use(command).Generate(), ServerCallContextFaker.CreateSend());

            // Assert
            _commandDispatcherMock.Verify(d => d.DispatchAsync(It.Is<TestCommand>(c => c.Name == command.Name), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact(DisplayName = "[UNIT][GRE-002]: Extract Request")]
        [GrpcEndpointFeature]
        public async Task GrpcEndpoint_SendAsync_ExtractRequest()
        {
            // Arrange
            var sut = CreateSUT();
            var callContext = ServerCallContextFaker.CreateSend();
            TestCommand command = new AutoFaker<TestCommand>().Generate();

            _deserializerMock.Setup(d => d.DeserializeAsync(It.IsAny<Envelop>(), It.IsAny<CancellationToken>())).ReturnsAsync(command);

            // Act
            await sut.SendAsync(new EnvelopFaker().Use(command).Generate(), callContext);

            // Assert
            _extractorMock.Verify(u => u.ExtractAsync(callContext.RequestHeaders, It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact(DisplayName = "[UNIT][GRE-003]: Receving Query")]
        [GrpcEndpointFeature]
        public async Task GrpcEndpoint_GetAsync_ReceivingQuery()
        {
            // Arrange
            var sut = CreateSUT();
            var callContext = ServerCallContextFaker.CreateGet();
            var query = new AutoFaker<TestQuery>().Generate();
            var result = new AutoFaker<TestQuery.Result>().Generate();

            _deserializerMock.Setup(d => d.DeserializeAsync(It.IsAny<Envelop>(), It.IsAny<CancellationToken>())).ReturnsAsync(query);
            _queryDispatcherMock.Setup(d => d.DispatchAsync<dynamic>(It.IsAny<TestQuery>(), It.IsAny<CancellationToken>())).ReturnsAsync(result);

            // Act
            var reply = await sut.GetAsync(new EnvelopFaker().Use<TestQuery, TestQuery.Result>(query).Generate(), callContext);

            // Assert
            Assert.Equal(new Reply() { Data = JsonSerializer.Serialize(result) }, reply);
        }

        [Fact(DisplayName = "[UNIT][GRE-004]: Handling Internal Errror (Command)")]
        [GrpcEndpointFeature]
        public async Task GrpcEndpoint_SendAsync_HandlingInternalError()
        {
            // Arrange
            var sut = CreateSUT();
            var exception = new AutoFaker<Exception>().Generate();
            var correlationId = Ulid.NewUlid();
            var options = new AutoFaker<ServiceOptions>().Generate();

            _requestContextMock.SetupGet(rca => rca.CorrelationId).Returns(correlationId);
            _deserializerMock.Setup().ReturnsAsync(new AutoFaker<TestCommand>().Generate());
            _commandDispatcherMock.Setup().ThrowsAsync(exception);
            _optionsMock.SetupGet(o => o.CurrentValue).Returns(options);

            // Act
            // Assert
            RpcException result = await Assert.ThrowsAsync<RpcException>(async () => await sut.SendAsync(new EnvelopFaker().Generate(), ServerCallContextFaker.CreateSend()));
            GrpcEndpointAssert.InternalError(result);
            GrpcEndpointAssert.ErrorInfo(result, options.Name, "INTERNAL_SERVER_ERROR", correlationId);
        }

        [Fact(DisplayName = "[UNIT][GRE-005]: Handling Not Found Error (Command)")]
        [GrpcEndpointFeature]
        public async Task GrpcEndpoint_SendAsync_HandlingNotFound()
        {
            // Arrange
            var sut = CreateSUT();
            var exception = new AutoFaker<NotFoundException>().Generate();
            var correlationId = Ulid.NewUlid();
            var options = new AutoFaker<ServiceOptions>().Generate();

            _requestContextMock.SetupGet(rca => rca.CorrelationId).Returns(correlationId);
            _deserializerMock.Setup().ReturnsAsync(new AutoFaker<TestCommand>().Generate());
            _commandDispatcherMock.Setup().ThrowsAsync(exception);
            _optionsMock.SetupGet(o => o.CurrentValue).Returns(options);

            // Act
            // Assert
            RpcException result = await Assert.ThrowsAsync<RpcException>(async () => await sut.SendAsync(new EnvelopFaker().Generate(), ServerCallContextFaker.CreateSend()));
            GrpcEndpointAssert.NotFound(exception, result);
            GrpcEndpointAssert.ErrorInfo(result, options.Name, "RESOURCE_NOT_FOUND", correlationId);
            GrpcEndpointAssert.ResourceInfo(exception, result, options.Name);
        }

        [Fact(DisplayName = "[UNIT][GRE-006]: Handling Conflict Error (Command)")]
        [GrpcEndpointFeature]
        public async Task GrpcEndpoint_SendAsync_HandlingConflictError()
        {
            // Arrange
            var sut = CreateSUT();
            var exception = new AutoFaker<ConflictException>().Generate();
            var correlationId = Ulid.NewUlid();
            var options = new AutoFaker<ServiceOptions>().Generate();

            _requestContextMock.SetupGet(rca => rca.CorrelationId).Returns(correlationId);
            _deserializerMock.Setup().ReturnsAsync(new AutoFaker<TestCommand>().Generate());
            _commandDispatcherMock.Setup().ThrowsAsync(exception);
            _optionsMock.SetupGet(o => o.CurrentValue).Returns(options);

            // Act
            // Assert
            RpcException result = await Assert.ThrowsAsync<RpcException>(async () => await sut.SendAsync(new EnvelopFaker().Generate(), ServerCallContextFaker.CreateSend()));
            GrpcEndpointAssert.Conflict(exception, result);
            GrpcEndpointAssert.ErrorInfo(result, options.Name, "CONFLICT", correlationId);
            GrpcEndpointAssert.ResourceInfo(exception, result, options.Name);
        }

        [Fact(DisplayName = "[UNIT][GRE-007]: Handling Business Rule Error (Command)")]
        [GrpcEndpointFeature]
        public async Task GrpcEndpoint_SendAsync_HandlingBusinessRuleError()
        {
            // Arrange
            var sut = CreateSUT();
            var exception = new AutoFaker<BusinessRuleException>().Generate();
            var correlationId = Ulid.NewUlid();
            var options = new AutoFaker<ServiceOptions>().Generate();

            _requestContextMock.SetupGet(rca => rca.CorrelationId).Returns(correlationId);
            _deserializerMock.Setup().ReturnsAsync(new AutoFaker<TestCommand>().Generate());
            _commandDispatcherMock.Setup().ThrowsAsync(exception);
            _optionsMock.SetupGet(o => o.CurrentValue).Returns(options);

            // Act
            // Assert
            RpcException result = await Assert.ThrowsAsync<RpcException>(async () => await sut.SendAsync(new EnvelopFaker().Generate(), ServerCallContextFaker.CreateSend()));
            GrpcEndpointAssert.BusinessRuleViolation(exception, result);
            GrpcEndpointAssert.ErrorInfo(result, options.Name, "BUSINESS_RULE_VIOLATION", correlationId);
            GrpcEndpointAssert.ResourceInfo(exception, result, options.Name);
        }

        [Fact(DisplayName = "[UNIT][GRE-008]: Handling Validation Failure (Command)")]
        [GrpcEndpointFeature]
        public async Task GrpcEndpoint_SendAsync_HandlingValidationFailure()
        {
            // Arrange
            var sut = CreateSUT();
            var exception = new AutoFaker<ValidationException>().Generate();
            var correlationId = Ulid.NewUlid();
            var options = new AutoFaker<ServiceOptions>().Generate();

            _requestContextMock.SetupGet(rca => rca.CorrelationId).Returns(correlationId);
            _deserializerMock.Setup().ReturnsAsync(new AutoFaker<TestCommand>().Generate());
            _commandDispatcherMock.Setup().ThrowsAsync(exception);
            _optionsMock.SetupGet(o => o.CurrentValue).Returns(options);

            // Act
            // Assert
            RpcException result = await Assert.ThrowsAsync<RpcException>(async () => await sut.SendAsync(new EnvelopFaker().Generate(), ServerCallContextFaker.CreateSend()));
            GrpcEndpointAssert.ValidationFailure(exception, result);
            GrpcEndpointAssert.ErrorInfo(result, options.Name, "VALIDATION_FAILURE", correlationId);
        }

        [Fact(DisplayName = "[UNIT][GRE-009]: Handling Internal Errror (Query)")]
        [GrpcEndpointFeature]
        public async Task GrpcEndpoint_GetAsync_HandlingInternalError()
        {
            // Arrange
            var sut = CreateSUT();
            var exception = new AutoFaker<Exception>().Generate();
            var correlationId = Ulid.NewUlid();
            var options = new AutoFaker<ServiceOptions>().Generate();

            _requestContextMock.SetupGet(rca => rca.CorrelationId).Returns(correlationId);
            _deserializerMock.Setup().ReturnsAsync(new AutoFaker<TestQuery>().Generate());
            _queryDispatcherMock.Setup().ThrowsAsync(exception);
            _optionsMock.SetupGet(o => o.CurrentValue).Returns(options);

            // Act
            // Assert
            RpcException result = await Assert.ThrowsAsync<RpcException>(async () => await sut.GetAsync(new EnvelopFaker().Generate(), ServerCallContextFaker.CreateSend()));
            GrpcEndpointAssert.InternalError(result);
            GrpcEndpointAssert.ErrorInfo(result, options.Name, "INTERNAL_SERVER_ERROR", correlationId);
        }

        [Fact(DisplayName = "[UNIT][GRE-010]: Handling Not Found Error (Query)")]
        [GrpcEndpointFeature]
        public async Task GrpcEndpoint_GetAsync_HandlingNotFound()
        {
            // Arrange
            var sut = CreateSUT();
            var exception = new AutoFaker<NotFoundException>().Generate();
            var correlationId = Ulid.NewUlid();
            var options = new AutoFaker<ServiceOptions>().Generate();

            _requestContextMock.SetupGet(rca => rca.CorrelationId).Returns(correlationId);
            _deserializerMock.Setup().ReturnsAsync(new AutoFaker<TestQuery>().Generate());
            _queryDispatcherMock.Setup().ThrowsAsync(exception);
            _optionsMock.SetupGet(o => o.CurrentValue).Returns(options);

            // Act
            // Assert
            RpcException result = await Assert.ThrowsAsync<RpcException>(async () => await sut.GetAsync(new EnvelopFaker().Generate(), ServerCallContextFaker.CreateSend()));
            GrpcEndpointAssert.NotFound(exception, result);
            GrpcEndpointAssert.ErrorInfo(result, options.Name, "RESOURCE_NOT_FOUND", correlationId);
            GrpcEndpointAssert.ResourceInfo(exception, result, options.Name);
        }

        [Fact(DisplayName = "[UNIT][GRE-011]: Handling Conflict Error (Query)")]
        [GrpcEndpointFeature]
        public async Task GrpcEndpoint_GetAsync_HandlingConflictError()
        {
            // Arrange
            var sut = CreateSUT();
            var exception = new AutoFaker<ConflictException>().Generate();
            var correlationId = Ulid.NewUlid();
            var options = new AutoFaker<ServiceOptions>().Generate();

            _requestContextMock.SetupGet(rca => rca.CorrelationId).Returns(correlationId);
            _deserializerMock.Setup().ReturnsAsync(new AutoFaker<TestQuery>().Generate());
            _queryDispatcherMock.Setup().ThrowsAsync(exception);
            _optionsMock.SetupGet(o => o.CurrentValue).Returns(options);

            // Act
            // Assert
            RpcException result = await Assert.ThrowsAsync<RpcException>(async () => await sut.GetAsync(new EnvelopFaker().Generate(), ServerCallContextFaker.CreateSend()));
            GrpcEndpointAssert.Conflict(exception, result);
            GrpcEndpointAssert.ErrorInfo(result, options.Name, "CONFLICT", correlationId);
            GrpcEndpointAssert.ResourceInfo(exception, result, options.Name);
        }

        [Fact(DisplayName = "[UNIT][GRE-012]: Handling Business Rule Error (Query)")]
        [GrpcEndpointFeature]
        public async Task GrpcEndpoint_QueryAsync_HandlingBusinessRuleError()
        {
            // Arrange
            var sut = CreateSUT();
            var exception = new AutoFaker<BusinessRuleException>().Generate();
            var correlationId = Ulid.NewUlid();
            var options = new AutoFaker<ServiceOptions>().Generate();

            _requestContextMock.SetupGet(rca => rca.CorrelationId).Returns(correlationId);
            _deserializerMock.Setup().ReturnsAsync(new AutoFaker<TestQuery>().Generate());
            _queryDispatcherMock.Setup().ThrowsAsync(exception);
            _optionsMock.SetupGet(o => o.CurrentValue).Returns(options);

            // Act
            // Assert
            RpcException result = await Assert.ThrowsAsync<RpcException>(async () => await sut.GetAsync(new EnvelopFaker().Generate(), ServerCallContextFaker.CreateSend()));
            GrpcEndpointAssert.BusinessRuleViolation(exception, result);
            GrpcEndpointAssert.ErrorInfo(result, options.Name, "BUSINESS_RULE_VIOLATION", correlationId);
            GrpcEndpointAssert.ResourceInfo(exception, result, options.Name);
        }

        [Fact(DisplayName = "[UNIT][GRE-013]: Handling Validation Failure (Query)")]
        [GrpcEndpointFeature]
        public async Task GrpcEndpoint_GetAsync_HandlingValidationFailure()
        {
            // Arrange
            var sut = CreateSUT();
            var exception = new AutoFaker<ValidationException>().Generate();
            var correlationId = Ulid.NewUlid();
            var options = new AutoFaker<ServiceOptions>().Generate();

            _requestContextMock.SetupGet(rca => rca.CorrelationId).Returns(correlationId);
            _deserializerMock.Setup().ReturnsAsync(new AutoFaker<TestQuery>().Generate());
            _queryDispatcherMock.Setup().ThrowsAsync(exception);
            _optionsMock.SetupGet(o => o.CurrentValue).Returns(options);

            // Act
            // Assert
            RpcException result = await Assert.ThrowsAsync<RpcException>(async () => await sut.GetAsync(new EnvelopFaker().Generate(), ServerCallContextFaker.CreateSend()));
            GrpcEndpointAssert.ValidationFailure(exception, result);
            GrpcEndpointAssert.ErrorInfo(result, options.Name, "VALIDATION_FAILURE", correlationId);
        }
    }

    [RequestVersion("v1")]
    file record TestCommand : Command
    {
        public required string Name { get; set; }
    }

    [RequestVersion("v1")]
    file record TestQuery : Query<TestQuery.Result>
    {
        public required string Name { get; set; }

        public record Result
        {
            public required string Message { get; init; }
        }
    }

    file static class GrpcEndpointAssert
    {
        public static void ValidationFailure(ValidationException expected, RpcException exception)
        {
            var status = exception.GetRpcStatus();
            var badRequest = status!.GetDetail<BadRequest>();

            Assert.Equal((int)StatusCode.InvalidArgument, status!.Code);
            Assert.Equal(expected.Message, status.Message);
            Assert.Collection(badRequest.FieldViolations, [.. expected.Errors.Inspect()]);
        }

        public static IEnumerable<Action<BadRequest.Types.FieldViolation>> Inspect(this IEnumerable<ValidationFailure> failures)
        {
            foreach (var failure in failures)
            {
                yield return (violation) =>
                {
                    Assert.Equal(failure.PropertyName, violation.Field);
                    Assert.Equal(failure.ErrorMessage, violation.Description);
                };
            }
        }

        public static void BusinessRuleViolation(BusinessRuleException expected, RpcException exception)
        {
            var status = exception.GetRpcStatus();
            var preconditionFailure = status!.GetDetail<PreconditionFailure>();

            Assert.Equal((int)StatusCode.FailedPrecondition, status!.Code);
            Assert.Equal(expected.Message, status.Message);
            Assert.Equal(expected.ErrorCode, preconditionFailure.Violations[0].Type);
            Assert.Equal(expected.Subject, preconditionFailure.Violations[0].Subject);
            Assert.Equal(expected.Message, preconditionFailure.Violations[0].Description);
        }
        public static void Conflict(ConflictException expected, RpcException exception)
        {
            var status = exception.GetRpcStatus();

            Assert.Equal((int)StatusCode.AlreadyExists, status!.Code);
            Assert.Equal(expected.Message, status.Message);
        }
        public static void NotFound(NotFoundException expected, RpcException exception)
        {
            var status = exception.GetRpcStatus();

            Assert.Equal((int)StatusCode.NotFound, status!.Code);
            Assert.Equal(expected.Message, status.Message);
        }

        public static void InternalError(RpcException exception)
        {
            var status = exception.GetRpcStatus();

            Assert.Equal((int)StatusCode.Internal, status!.Code);
            Assert.Equal("An internal server error occured", status.Message);
        }

        public static void ErrorInfo(RpcException exception, string domain, string reason, Ulid correlationId)
        {
            var errorInfo = exception.GetRpcStatus()!.GetDetail<ErrorInfo>();

            Assert.Equal(domain, errorInfo.Domain);
            Assert.Equal(reason, errorInfo.Reason);
            Assert.Equal(correlationId, Ulid.Parse(errorInfo.Metadata["Ironyx.ErrorInfo.CorrelationId"]));
        }

        public static void ResourceInfo(ResourceException expected, RpcException exception, string owner)
        {
            var resourceInfo = exception.GetRpcStatus()!.GetDetail<ResourceInfo>();

            Assert.Equal(owner, resourceInfo.Owner);
            Assert.Equal(expected.ResourceType, resourceInfo.ResourceType);
            Assert.Equal(expected.ResourceName, resourceInfo.ResourceName);
            Assert.Equal(expected.Message, resourceInfo.Description);
        }
    }

    file static class GrpcEndpointTestExtensions
    {
        public static ISetup<IRequestDeserializer, Task<dynamic>> Setup(this Mock<IRequestDeserializer> mock)
        {
            return mock.Setup(d => d.DeserializeAsync(It.IsAny<Envelop>(), It.IsAny<CancellationToken>()));
        }

        public static ISetup<ICommandDispatcher, Task> Setup(this Mock<ICommandDispatcher> mock)
        {
            return mock.Setup(d => d.DispatchAsync(It.IsAny<TestCommand>(), It.IsAny<CancellationToken>()));
        }

        public static ISetup<IQueryDispatcher, Task<dynamic>> Setup(this Mock<IQueryDispatcher> mock)
        {
            return mock.Setup(d => d.DispatchAsync<dynamic>(It.IsAny<TestQuery>(), It.IsAny<CancellationToken>()));
        }
    }
}
