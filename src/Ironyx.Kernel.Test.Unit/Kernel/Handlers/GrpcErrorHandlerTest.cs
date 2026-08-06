using Google.Rpc;
using Grpc.Core;
using Ironyx.Kernel.Handlers;
using Ironyx.Kernel.Test.Unit.Kernel.Fakers;
using Microsoft.Extensions.Logging;
using System.Collections;
using Xunit.Abstractions;

namespace Ironyx.Kernel.Test.Unit.Kernel.Handlers
{
    public class GrpcErrorHandlerTest
    {

        private readonly ILogger<GrpcErrorHandler> _logger;

        public GrpcErrorHandlerTest(ITestOutputHelper outputHelper)
        {
            _logger = new LoggerFactory()
                          .AddXUnit(outputHelper)
                          .CreateLogger<GrpcErrorHandler>();
        }

        private GrpcErrorHandler CreateSUT()
        {
            return new GrpcErrorHandler(_logger);
        }

        [Fact(DisplayName = "[UNIT][GEH-001]: Handle Internal Server Error")]
        [ErrorHandlingFeature]
        public void GrpcErrorHandler_Handle_HandleInternalServerError()
        {
            // Arrange
            var sut = CreateSUT();
            var status = new StatusFaker().InternalServerError().Generate();

            // Act
            // Assert
            var result = Assert.Throws<InvalidOperationException>(() => sut.Handle(status.ToRpcException()));
            Assert.Equal("An internal server error occured", result.Message);
            GrpcErrorHandlerAssert.ErrorInfo(status.GetDetail<ErrorInfo>(), result.Data, "INTERNAL_SERVER_ERROR");
        }

        [Fact(DisplayName = "[UNIT][GEH-002]: Handle Not Found")]
        [ErrorHandlingFeature]
        public void GrpcErrorHandler_Handle_HandleNotFound()
        {
            // Arrange
            var sut = CreateSUT();
            var status = new StatusFaker().NotFound().Generate();

            // Act
            // Assert
            var result = Assert.Throws<NotFoundException>(() => sut.Handle(status.ToRpcException()));
            Assert.Equal(status.Message, result.Message);
            GrpcErrorHandlerAssert.ErrorInfo(status.GetDetail<ErrorInfo>(), result.Data, "RESOURCE_NOT_FOUND");
            GrpcErrorHandlerAssert.ResourceInfo(status.GetDetail<ResourceInfo>(), result.Data);
        }

        [Fact(DisplayName = "[UNIT][GEH-003]: Handle Conflict")]
        [ErrorHandlingFeature]
        public void GrpcErrorHandler_Handle_HandleConflict()
        {
            // Arrange
            var sut = CreateSUT();
            var status = new StatusFaker().Conflict().Generate();

            // Act
            // Assert
            var result = Assert.Throws<ConflictException>(() => sut.Handle(status.ToRpcException()));
            Assert.Equal(status.Message, result.Message);
            GrpcErrorHandlerAssert.ErrorInfo(status.GetDetail<ErrorInfo>(), result.Data, "CONFLICT");
            GrpcErrorHandlerAssert.ResourceInfo(status.GetDetail<ResourceInfo>(), result.Data);
        }

        [Fact(DisplayName = "[UNIT][GEH-004]: Handle Business Rule Error")]
        [ErrorHandlingFeature]
        public void GrpcErrorHandler_Handle_HandleBusinessRuleError()
        {
            // Arrange
            var sut = CreateSUT();
            var status = new StatusFaker().BusinessRule().Generate();

            // Act
            // Assert
            var result = Assert.Throws<BusinessRuleException>(() => sut.Handle(status.ToRpcException()));
            Assert.Equal(status.Message, result.Message);
            GrpcErrorHandlerAssert.ErrorInfo(status.GetDetail<ErrorInfo>(), result.Data, "BUSINESS_RULE_VIOLATION");
            GrpcErrorHandlerAssert.ResourceInfo(status.GetDetail<ResourceInfo>(), result.Data);
            GrpcErrorHandlerAssert.PreconditionFailure(status.GetDetail<Google.Rpc.PreconditionFailure>(), result.Data);
        }
    }

    file static class GrpcErrorHandlerAssert
    {
        public static void PreconditionFailure(PreconditionFailure expected, IDictionary actual)
        {
            Assert.Equal(expected.Violations[0].Type, actual["Ironyx.BusinessViolation.Type"]);
            Assert.Equal(expected.Violations[0].Subject, actual["Ironyx.BusinessViolation.Subject"]);
            Assert.Equal(expected.Violations[0].Description, actual["Ironyx.BusinessViolation.Description"]);
        }

        public static void ErrorInfo(ErrorInfo expected, IDictionary actual, string reason)
        {
            Assert.Equal(expected.Domain, actual["Ironyx.ErrorInfo.Domain"]);
            Assert.Equal(reason, actual["Ironyx.ErrorInfo.Reason"]);
            Assert.Equal(expected.Metadata["Ironyx.ErrorInfo.CorrelationId"], actual["Ironyx.ErrorInfo.CorrelationId"]);
        }

        public static void ResourceInfo(ResourceInfo expected, IDictionary actual)
        {
            Assert.Equal(expected.Owner, actual["Ironyx.ResourceInfo.Owner"]);
            Assert.Equal(expected.ResourceType, actual["Ironyx.ResourceInfo.ResourceType"]);
            Assert.Equal(expected.ResourceName, actual["Ironyx.ResourceInfo.ResourceName"]);
            Assert.Equal(expected.Description, actual["Ironyx.ResourceInfo.Description"]);
        }
    }
}
