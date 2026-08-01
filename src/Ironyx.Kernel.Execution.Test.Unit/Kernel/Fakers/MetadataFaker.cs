using Grpc.Core;

namespace Ironyx.Kernel.Test.Unit.Kernel.Fakers
{
    public class MetadataFaker
    {
        private readonly Metadata _metadata = [];

        public MetadataFaker WithCausationId(Ulid value)
        {
            _metadata.Add("causation-id", value.ToString());

            return this;
        }

        public MetadataFaker WithCorrelationId(Ulid value)
        {
            _metadata.Add("correlation-id", value.ToString());

            return this;
        }

        public Metadata Generate()
        {
            return _metadata;
        }
    }
}
