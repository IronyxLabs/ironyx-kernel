namespace Ironyx.Kernel.Unwrappers
{
    public interface IRequestDeserializer
    {
        Task<dynamic> DeserializeAsync(Request request, CancellationToken cancellationToken);
    }
}
