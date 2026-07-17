using Bogus;
using Grpc.Core;
using Grpc.Core.Testing;

namespace Ironyx.Kernel.Test.Unit.Fakers
{
    internal static class ServerCallContextFaker
    {
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
