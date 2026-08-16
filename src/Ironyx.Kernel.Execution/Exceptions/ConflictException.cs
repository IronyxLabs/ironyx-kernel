using Ironyx.Kernel.Execution.Exceptions;

namespace Ironyx.Kernel
{
    public class ConflictException : ResourceException
    {
        public ConflictException() : base()
        {
        }

        public ConflictException(string? message) : base(message)
        {
        }

        public ConflictException(string? message, Exception? innerException) : base(message, innerException)
        {
        }
    }
}
