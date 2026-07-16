namespace Ironyx.Kernel.Execution.Contexts
{
    public class RequestContext : IRequestContext, IRequestContextAccessor
    {
        public Ulid RequestId { get; set; }
        public Ulid? CausationId { get; set; }
        public Ulid CorrelationId { get; set; }
    }
}
