namespace Ironyx.Kernel.Registry
{
    public interface ICanonicalTypeResolver
    {
        CanonicalTypeDescription this[Type type] { get; }
    }
}
