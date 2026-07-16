using System.Diagnostics.CodeAnalysis;

namespace Ironyx.Kernel.Generators
{
    public interface IUlidGenerator
    {
        Ulid Get();
    }

    [ExcludeFromCodeCoverage]
    public class ULidGenerator : IUlidGenerator
    {
        public Ulid Get()
        {
            return Ulid.NewUlid();
        }
    }
}
