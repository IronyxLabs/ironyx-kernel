namespace Ironyx.Kernel.Registry
{
    public interface IRuntimeTypeResolver
    {
        Type this[string type, string version] { get; }
    }
}
