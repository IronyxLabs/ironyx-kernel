namespace Ironyx.Kernel.Execution.Registries
{
    public interface IHandlerRegistry
    {
        Type this[Type type] { get; }

        void Add(Type command, Type handler);
    }
}
