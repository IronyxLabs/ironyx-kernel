namespace Ironyx.Kernel
{
    public class NotFoundException : Exception
    {
        public string? ResourceType { get; set; }
        public string? ResourceName { get; set; }

        public NotFoundException() : base()
        {
        }

        public NotFoundException(string? message) : base(message)
        {
        }

        public NotFoundException(string? message, Exception? innerException) : base(message, innerException)
        {
        }
    }
}
