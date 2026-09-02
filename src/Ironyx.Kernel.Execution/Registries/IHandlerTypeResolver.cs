namespace Ironyx.Kernel.Execution.Registries
{
    public record HandlerTypeDescription
    {
        public IEnumerable<Type> PreHandlers { get; init; } = [];
        public required Type Handler { get; init; }
    }

    public interface IHandlerTypeResolver
    {
        HandlerTypeDescription this[Type type] { get; }
    }
}
