namespace Ironyx.Kernel.Execution.Test.Unit.Endpoints
{
    public class GrpcEndpointTest
    {

        //private ILogger<GrpcEndpoint> _logger;
        //private Mock<IRequestDeserializer> _deserializerMock;
        //private Mock<IUnwrapper> _unwrapperMock;
        //private Mock<IRequestContextAccessor> _requestContextMock;
        //private Mock<ICommandDispatcher> _dispatcherMock;

        //public GrpcEndpointTest(ITestOutputHelper outputHelper)
        //{
        //    _logger = new LoggerFactory()
        //                  .AddXUnit(outputHelper)
        //                  .CreateLogger<GrpcEndpoint>();
        //}

        //private GrpcEndpoint CreateSUT()
        //{
        //    _deserializerMock = new Mock<IRequestDeserializer>();
        //    _unwrapperMock = new Mock<IUnwrapper>();
        //    _requestContextMock = new Mock<IRequestContextAccessor>();
        //    _dispatcherMock = new Mock<ICommandDispatcher>();

        //    return new GrpcEndpoint(_deserializerMock.Object, _unwrapperMock.Object, _requestContextMock.Object, _dispatcherMock.Object, _logger);
        //}

        //[Fact(DisplayName = "[UNIT][GRE-001]: Receiving Command")]
        //[Feature("GRE", "GRPC Endpoint")]
        //public async Task GrpcEndpoint_SendAsync_ReceivingCommand()
        //{
        //    // Arrange
        //    var sut = CreateSUT();
        //    var command = new AutoFaker<TestCommand>().Generate();

        //    _deserializerMock.Setup(d => d.DeserializeAsync(It.IsAny<Request>(), It.IsAny<CancellationToken>())).ReturnsAsync(command);

        //    // Act
        //    await sut.SendAsync(new Request(), ServerCallContextFaker.CreateSend());

        //    // Assert
        //    _dispatcherMock.Verify(d => d.DispatchAsync<Command>(It.Is<TestCommand>(c => c.Name == command.Name), It.IsAny<CancellationToken>()), Times.Once);
        //}

        //[Fact(DisplayName = "[UNIT][GRE-002]: Unwrap Request")]
        //[Feature("GRE", "GRPC Endpoint")]
        //public async Task GrpcEndpoint_SendAsync_UnwrapRequest()
        //{
        //    // Arrange
        //    var sut = CreateSUT();
        //    var callContext = ServerCallContextFaker.CreateSend();

        //    _deserializerMock.Setup(d => d.DeserializeAsync(It.IsAny<Request>(), It.IsAny<CancellationToken>())).ReturnsAsync(new AutoFaker<TestCommand>().Generate());

        //    // Act
        //    await sut.SendAsync(new Request(), callContext);

        //    // Assert
        //    _unwrapperMock.Verify(u => u.UnwrapAsync(callContext.RequestHeaders, It.IsAny<CancellationToken>()), Times.Once);
        //}

        //[Fact(DisplayName = "[UNIT][GRE-003]: Accept Request")]
        //[Feature("GRE", "GRPC Endpoint")]
        //public async Task GrpcEndpoint_SendAsync_AcceptRequest()
        //{
        //    // Arrange
        //    var sut = CreateSUT();
        //    var callContext = ServerCallContextFaker.CreateSend();

        //    _deserializerMock.Setup(d => d.DeserializeAsync(It.IsAny<Request>(), It.IsAny<CancellationToken>())).ReturnsAsync(new AutoFaker<TestCommand>().Generate());

        //    // Act
        //    var reply = await sut.SendAsync(new Request(), callContext);

        //    // Assert
        //    Assert.Equal(new Reply() { Status = "ACCEPTED" }, reply);
        //}

        //[Fact(DisplayName = "[UNIT][GRE-004]: Type is not Defined")]
        //[Feature("GRE", "GRPC Endpoint")]
        //public async Task GrpcEndpoint_SendAsync_TypeIsNotDefined()
        //{
        //    // Arrange
        //    var sut = CreateSUT();
        //    var callContext = ServerCallContextFaker.CreateSend();

        //    _deserializerMock.Setup(d => d.DeserializeAsync(It.IsAny<Request>(), It.IsAny<CancellationToken>())).ThrowsAsync(new ArgumentNullException());

        //    // Act
        //    var reply = await sut.SendAsync(new Request(), callContext);

        //    // Assert
        //    Assert.Equal(new Reply() { Status = "ERROR", Error = new Error { Code = "TECH_REQUEST_TYPE_IS_MISSING", Message = "The 'request-type' header is not defined" } }, reply);
        //    Assert.Equal(callContext.Status, new Status(StatusCode.InvalidArgument, "The 'request-type' header is not defined"));
        //}

        //[Fact(DisplayName = "[UNIT][GRE-005]: Unknown Request Type")]
        //[Feature("GRE", "GRPC Endpoint")]
        //public async Task GrpcEndpoint_SendAsync_UnknownRequestType()
        //{
        //    // Arrange
        //    var sut = CreateSUT();
        //    var callContext = ServerCallContextFaker.CreateSend();

        //    _deserializerMock.Setup(d => d.DeserializeAsync(It.IsAny<Request>(), It.IsAny<CancellationToken>())).ThrowsAsync(new NotSupportedException());

        //    // Act
        //    var reply = await sut.SendAsync(new Request(), callContext);

        //    // Assert
        //    Assert.Equal(new Reply() { Status = "ERROR", Error = new Error { Code = "TECH_UNKNOWN_REQUEST_TYPE", Message = "Unknow request type" } }, reply);
        //    Assert.Equal(callContext.Status, new Status(StatusCode.InvalidArgument, "Unknow request type"));
        //}

        //[Fact(DisplayName = "[UNIT][GRE-006]: Serialization Error")]
        //[Feature("GRE", "GRPC Endpoint")]
        //public async Task GrpcEndpoint_SendAsync_SerializationError()
        //{
        //    // Arrange
        //    var sut = CreateSUT();
        //    var callContext = ServerCallContextFaker.CreateSend();

        //    _deserializerMock.Setup(d => d.DeserializeAsync(It.IsAny<Request>(), It.IsAny<CancellationToken>())).ThrowsAsync(new JsonException());

        //    // Act
        //    var reply = await sut.SendAsync(new Request(), callContext);

        //    // Assert
        //    Assert.Equal(new Reply() { Status = "ERROR", Error = new Error { Code = "TECH_INVALID_REQUEST_BODY", Message = "Invalid request body" } }, reply);
        //    Assert.Equal(callContext.Status, new Status(StatusCode.InvalidArgument, "Invalid request body"));
        //}
    }

    file record TestCommand : Command
    {
        public required string Name { get; set; }
    }
}
