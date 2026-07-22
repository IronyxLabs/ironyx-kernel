namespace Ironyx.Kernel.Serializers
{
    public interface IRequestDeserializer
    {
        Task<dynamic> DeserializeAsync(Envelop envelop, CancellationToken cancellationToken);
    }
}
