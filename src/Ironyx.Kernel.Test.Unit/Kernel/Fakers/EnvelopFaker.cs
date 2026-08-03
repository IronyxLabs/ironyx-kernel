using Bogus;
using Castle.Core.Internal;
using System.Text.Json;

namespace Ironyx.Kernel.Test.Unit.Kernel.Fakers
{
    public class EnvelopFaker
    {
        private string? _type;
        private string? _version;
        private string? _payload;

        public EnvelopFaker Use<TCommand>(TCommand command)
            where TCommand : Command
        {
            _type = typeof(TCommand).FullName;
            _version = typeof(TCommand).GetAttribute<RequestVersionAttribute>().Version;
            _payload = JsonSerializer.Serialize(command);

            return this;
        }

        public EnvelopFaker Use<TQuery, TResult>(TQuery query)
            where TQuery : Query<TResult>
        {
            _type = typeof(TQuery).FullName;
            _version = typeof(TQuery).GetAttribute<RequestVersionAttribute>().Version;
            _payload = JsonSerializer.Serialize(query);

            return this;
        }

        public EnvelopFaker WithoutType()
        {
            _type = null;

            return this;
        }

        public EnvelopFaker WithoutVersion()
        {
            _version = null;

            return this;
        }

        public Envelop Generate()
        {
            var faker = new Faker();

            _type = faker.Random.String();
            _version = faker.Random.String();
            _payload = faker.Random.String();

            return new Envelop { Type = _type, Version = _version, Payload = _payload };
        }
    }
}
