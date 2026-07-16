namespace Ironyx.Kernel
{
    public interface IRequestContextAccessor
    {
        public Ulid RequestId { get; }
        public Ulid? CausationId { get; }
        public Ulid CorrelationId { get; }
    }
}
