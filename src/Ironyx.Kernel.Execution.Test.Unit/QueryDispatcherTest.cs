using AutoBogus;
using Bogus;
using Ironyx.Kernel.Execution.Dispatchers;
using Ironyx.Kernel.Test.Features;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Xunit.Abstractions;

namespace Ironyx.Kernel.Execution.Test.Unit
{
    public class QueryDispatcherTest
    {
        private readonly ILogger<QueryDispatcher> _logger;

        public QueryDispatcherTest(ITestOutputHelper outputHelper)
        {
            _logger = new LoggerFactory()
                          .AddXUnit(outputHelper)
                          .CreateLogger<QueryDispatcher>();
        }

        private QueryDispatcher CreateSUT()
        {
            return new QueryDispatcher(new ServiceCollection().BuildServiceProvider(), _logger);
        }

        private QueryDispatcher CreateSUT(Func<string> action)
        {
            var serviceCollection = new ServiceCollection();
            serviceCollection.AddSingleton<IQueryHandler<TestQuery, string>>(new TestQueryHandler(action));

            return new QueryDispatcher(serviceCollection.BuildServiceProvider(), _logger);
        }

        [Fact(DisplayName = "[UNIT][QRD-001]: Dispatch Query")]
        [QueryHandlingFeature]
        public async Task QueryDispatcher_DispatchAsync_DispatchQuery()
        {
            // Arrange
            var value = new Faker().Random.String2(10);
            var sut = CreateSUT(() => value);

            // Act
            var result = await sut.DispatchAsync(new AutoFaker<TestQuery>().Generate(), default);

            // Assert
            Assert.Equal(value, result);
        }

        [Fact(DisplayName = "[UNIT][QRD-002]: Handler not Found")]
        [QueryHandlingFeature]
        public async Task QueryDispatcher_DispatchAsync_HandlerNotFound()
        {
            // Arrange
            var sut = CreateSUT();

            // Act
            // Assert
            await Assert.ThrowsAsync<InvalidOperationException>(async () => await sut.DispatchAsync(new AutoFaker<TestQuery>().Generate(), default));
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
}
