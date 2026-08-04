namespace Ironyx.Kernel.Handlers
{
    public interface IErrorHandler<TException>
        where TException : Exception
    {
        void Handle(TException exception);
    }
}
