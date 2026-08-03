namespace Ironyx.Kernel
{
    public class BusinessRuleException : Exception
    {
        public string? ErrorCode { get; init; }

        public BusinessRuleException(string? errorCode, string message) : base(message)
        {
            ErrorCode = errorCode;
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
