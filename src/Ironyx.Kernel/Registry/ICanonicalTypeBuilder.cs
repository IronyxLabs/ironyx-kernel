namespace Ironyx.Kernel.Registry
{
    public interface ICanonicalTypeBuilder
    {
        void Add<TCommand>()
            where TCommand : Command;
    }
}
