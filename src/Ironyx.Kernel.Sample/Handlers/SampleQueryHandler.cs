using Ironyx.Kernel.Execution;
using Ironyx.Kernel.Execution.Senders;

namespace Ironyx.Kernel.Sample.Handlers
{
    [RequestVersion("v1")]
    public record SampleQuery : Query<SampleQuery.Result>
    {
        public required string Name { get; init; }

        public record Result
        {
            public required string Message { get; init; }
        }
    }

    public class SampleQueryHandler : IQueryHandler<SampleQuery, SampleQuery.Result>
    {
        private readonly IRequestSender _sender;

        public SampleQueryHandler(IRequestSender sender)
        {
            _sender = sender;
        }

        public async Task<SampleQuery.Result> HandleAsync(SampleQuery query, CancellationToken cancellationToken)
        {
            if (query.Name == "FORWARD")
            {
                return (await _sender.GetAsync<SampleQuery, SampleQuery.Result>(new SampleQuery { Name = "Hello" }, cancellationToken))!;
            }

            throw new BusinessRuleException("BUSS_001", "Business Exception");
            return new SampleQuery.Result { Message = $"Hello {query.Name}" };
        }
    }
}
