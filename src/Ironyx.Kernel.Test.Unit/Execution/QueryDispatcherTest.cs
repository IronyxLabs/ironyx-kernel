using AutoBogus;
using Bogus;
using Ironyx.Kernel.Execution;
using Ironyx.Kernel.Execution.Behaviors;
using Ironyx.Kernel.Execution.Dispatchers;
using Ironyx.Kernel.Execution.Registries;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit.Abstractions;

namespace Ironyx.Kernel.Test.Unit.Execution
{
    public class QueryDispatcherTest
    {
        private readonly ILogger<QueryDispatcher> _logger;
        private Mock<IHandlerTypeResolver> _resolverMock = null!;

        public QueryDispatcherTest(ITestOutputHelper outputHelper)
        {
            _logger = new LoggerFactory()
                          .AddXUnit(outputHelper)
                          .CreateLogger<QueryDispatcher>();
        }

        private QueryDispatcher CreateSUT(Func<string> action, Action? preAction = null)
        {
            var serviceCollection = new ServiceCollection();
            serviceCollection.AddSingleton(new TestQueryHandler(action));
            if (preAction is not null) serviceCollection.AddSingleton(new PreTestQueryHandler(preAction));

            _resolverMock = new Mock<IHandlerTypeResolver>();

            return new QueryDispatcher(serviceCollection.BuildServiceProvider(), _resolverMock.Object, _logger);
        }

        [Fact(DisplayName = "[UNIT][QRD-001]: Dispatch Query")]
        [QueryHandlingFeature]
        public async Task QueryDispatcher_DispatchAsync_DispatchQuery()
        {
            // Arrange
            var value = new Faker().Random.String2(10);
            var sut = CreateSUT(() => value);

            _resolverMock.SetupGet(r => r[typeof(TestQuery)]).Returns(new HandlerTypeDescription { Handler = typeof(TestQueryHandler) });

            // Act
            var result = await sut.DispatchAsync<string>(new AutoFaker<TestQuery>().Generate(), default);

            // Assert
            Assert.Equal(value, result);
        }

        [Fact(DisplayName = "[UNIT][QRD-001]: Use PreHandlers")]
        [QueryHandlingFeature]
        public async Task QueryDispatcher_DispatchAsync_UsePreHandler()
        {
            // Arrange
            var called = false;
            var sut = CreateSUT(() => { return ""; }, () => called = true);

            _resolverMock.SetupGet(r => r[typeof(TestQuery)]).Returns(new HandlerTypeDescription { Handler = typeof(TestQueryHandler), PreHandlers = [typeof(PreTestQueryHandler)] });

            // Act
            var result = await sut.DispatchAsync<string>(new AutoFaker<TestQuery>().Generate(), default);

            // Assert
            Assert.True(called);
        }
    }

    file record TestQuery : Query<string> { }

    file class TestQueryHandler : IQueryHandler<TestQuery, string>
    {
        private readonly Func<string> _action;

        public TestQueryHandler(Func<string> action)
        {
            _action = action;
        }

        public Task<string> HandleAsync(TestQuery query, CancellationToken cancellationToken)
        {
            return Task.FromResult(_action());
        }
    }

    file class PreTestQueryHandler : IPreHandler<TestQuery>
    {
        private readonly Action _action;

        public PreTestQueryHandler(Action action)
        {
            _action = action;
        }

        public Task HandleAsync(TestQuery request, CancellationToken cancellationToken = default)
        {
            _action();

            return Task.CompletedTask;
        }
    }
}
