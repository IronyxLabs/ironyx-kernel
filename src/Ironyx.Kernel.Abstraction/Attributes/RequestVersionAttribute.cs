namespace Ironyx.Kernel
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public class RequestVersionAttribute : Attribute
    {
        public string Version { get; }

        public RequestVersionAttribute(string version)
        {
            Version = version;
        }
    }
}
