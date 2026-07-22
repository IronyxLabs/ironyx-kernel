namespace Ironyx.Kernel.Execution.Registries
{
    public interface IHandlerTypeResolver
    {
        Type this[Type type] { get; }
    }
}
