namespace Ironyx.Kernel.Generators
{
    public interface IUlidGenerator
    {
        Ulid Get();
    }

    public class ULidGenerator : IUlidGenerator
    {
        public Ulid Get()
        {
            return Ulid.NewUlid();
        }
    }
}
