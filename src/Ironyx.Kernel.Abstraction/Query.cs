using Ironyx.Kernel.Abstraction.Interfaces;

namespace Ironyx.Kernel
{
    public abstract record Query<TResult> : IQuery
    {
    }
}
