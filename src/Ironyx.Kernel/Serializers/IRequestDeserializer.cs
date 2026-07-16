namespace Ironyx.Kernel.Serializers
{
    public interface IRequestDeserializer
    {
        Task<dynamic> DeserializeAsync(Request request, CancellationToken cancellationToken);
    }
}
