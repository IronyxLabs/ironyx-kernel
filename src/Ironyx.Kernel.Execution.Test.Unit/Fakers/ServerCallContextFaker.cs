using Bogus;
using Grpc.Core;
using Grpc.Core.Testing;

namespace Ironyx.Kernel.Execution.Test.Unit.Fakers
{
    internal static class ServerCallContextFaker
    {
        public static ServerCallContext CreateSend<TCommand>()
            where TCommand : Command
        {
            var faker = new Faker();

            return TestServerCallContext.Create(
                "SendAsync",
                faker.Internet.Ip(),
                faker.Date.Future(),
                new Metadata().SetRequestType<TCommand>(),
                default,
                faker.Internet.Ip(),
                new AuthContext(null, []),
                null,
                null, null, null);
        }


        public static ServerCallContext CreateSend()
        {
            var faker = new Faker();

            return TestServerCallContext.Create(
                "SendAsync",
                faker.Internet.Ip(),
                faker.Date.Future(),
                [],
                default,
                faker.Internet.Ip(),
                new AuthContext(null, []),
                null,
                null, null, null);
        }


        public static ServerCallContext CreateSend(string requestType)
        {
            var faker = new Faker();

            var metadata = new Metadata
            {
                { "request-type", requestType }
            };

            return TestServerCallContext.Create(
                "SendAsync",
                faker.Internet.Ip(),
                faker.Date.Future(),
                metadata,
                default,
                faker.Internet.Ip(),
                new AuthContext(null, []),
                null,
                null, null, null);
        }
    }

    file static class ServerCallContextFakerExtensions
    {
        public static Metadata SetRequestType<TCommand>(this Metadata headers)
            where TCommand : Command
        {
            headers.Add("request-type", $"{typeof(TCommand).FullName}, {typeof(TCommand).Assembly.GetName().Name}");

            return headers;
        }
    }
}
