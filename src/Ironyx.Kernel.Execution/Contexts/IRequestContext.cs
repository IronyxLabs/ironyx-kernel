namespace Ironyx.Kernel.Execution.Contexts
{
    public interface IRequestContext
    {
        public Ulid RequestId { get; set; }
        public Ulid? CausationId { get; set; }
        public Ulid CorrelationId { get; set; }
    }
}
