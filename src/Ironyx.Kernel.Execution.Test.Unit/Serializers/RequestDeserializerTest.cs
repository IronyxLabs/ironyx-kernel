namespace Ironyx.Kernel.Execution.Test.Unit.Serializers
{
    public class RequestDeserializerTest
    {

        //private ILogger<RequestDeserializer> _logger;

        //public RequestDeserializerTest(ITestOutputHelper outputHelper)
        //{
        //    _logger = new LoggerFactory()
        //                  .AddXUnit(outputHelper)
        //                  .CreateLogger<RequestDeserializer>();
        //}

        //private RequestDeserializer CreateSUT()
        //{
        //    return new RequestDeserializer(_logger);
        //}

        //[Fact(DisplayName = "[UNIT][RQU-001]: Deserialize Request")]
        //[Feature("CMD", "Command Handling")]
        //public async Task RequestDeserializer_DeserializeAsync_DeserializeRequest()
        //{
        //    // Arrange
        //    var sut = CreateSUT();
        //    var command = new AutoFaker<TestCommand>().Generate();

        //    // Act
        //    var result = await sut.DeserializeAsync(new RequestFaker().With(command).Generate(), default);

        //    // Assert
        //    Assert.Equal(command, result);
        //}

        //[Fact(DisplayName = "[UNIT][RQU-002]: Request Type is not Defined")]
        //[Feature("CMD", "Command Handling")]
        //public async Task RequestUnwrapper_UnwrapAsync_RequestTypeIsNotDefined()
        //{
        //    // Arrange
        //    var sut = CreateSUT();
        //    var command = new AutoFaker<TestCommand>().Generate();

        //    // Act
        //    // Assert
        //    await Assert.ThrowsAsync<ArgumentNullException>(async () => await sut.DeserializeAsync(new RequestFaker().WithoutRequestType(command).Generate(), default));
        //}

        //[Fact(DisplayName = "[UNIT][RQU-002]: Unknow Type is Defined")]
        //[Feature("CMD", "Command Handling")]
        //public async Task RequestUnwrapper_UnwrapAsync_UnknownTypeIsDefined()
        //{
        //    // Arrange
        //    var sut = CreateSUT();
        //    var command = new AutoFaker<TestCommand>().Generate();

        //    // Act
        //    // Assert
        //    await Assert.ThrowsAsync<NotSupportedException>(async () => await sut.DeserializeAsync(new RequestFaker().WithType(new Faker().Random.String2(10)).Generate(), default));
        //}
    }

    file record TestCommand : Command
    {
        public string Message { get; init; } = null!;
    }
}
