namespace Ironyx.Kernel.Execution.Exceptions
{
    public class ResourceException : Exception
    {
        public string? ResourceType { get; set; }
        public string? ResourceName { get; set; }

        protected ResourceException() : base()
        {
        }

        protected ResourceException(string? message) : base(message)
        {
        }

        protected ResourceException(string? message, Exception? innerException) : base(message, innerException)
        {
        }
    }
}
