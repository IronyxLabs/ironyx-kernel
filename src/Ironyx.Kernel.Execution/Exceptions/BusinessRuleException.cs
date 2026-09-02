using Ironyx.Kernel.Execution.Exceptions;

namespace Ironyx.Kernel
{
    public class BusinessRuleException : ResourceException
    {
        public string? ErrorCode { get; init; }
        public string? Subject { get; init; }

        public BusinessRuleException(string? errorCode, string? subject, string message) : base(message)
        {
            ErrorCode = errorCode;
            Subject = subject;
        }

        public BusinessRuleException() : base()
        {
        }

        public BusinessRuleException(string? message) : base(message)
        {
        }

        public BusinessRuleException(string? message, Exception? innerException) : base(message, innerException)
        {
        }
    }
}
