namespace Ironyx.Kernel.Execution.Registries
{
    public interface IHandlerRegistry
    {
        HandlerTypeDescription this[Type type] { get; }

        void Add(Type command, Type handler, IEnumerable<Type> preHandlers);
    }
}
