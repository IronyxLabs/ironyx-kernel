using System.Reflection;

namespace Ironyx.Kernel.Registry
{
    public record CanonicalTypeDescription
    {
        public required string Type { get; init; }
        public required string Version { get; init; }

        public static CanonicalTypeDescription Create(string type, string version)
        {
            return new CanonicalTypeDescription { Type = type, Version = version };
        }
    }

    public class CanonicalTypeRegistry : ICanonicalTypeBuilder, IRuntimeTypeResolver
    {
        public IDictionary<CanonicalTypeDescription, Type> Registrations { get; } = new Dictionary<CanonicalTypeDescription, Type>();

        public Type this[string type, string version]
        {
            get
            {
                var description = CanonicalTypeDescription.Create(type, version);
                if (!Registrations.TryGetValue(description, out var result)) throw Exceptions.NotSupported(description.Type, description.Version);
                return result;
            }
        }

        public void Add(Type type)
        {
            var attribute = type.GetCustomAttribute<RequestVersionAttribute>() ?? throw Exceptions.VersionNotDefined(type.FullName!);

            var description = CanonicalTypeDescription.Create(type.FullName!, attribute.Version);
            if (Registrations.ContainsKey(description)) throw Exceptions.Conflict(description.Version, description.Type);

            Registrations.Add(description, type);
        }
    }

    file static class Exceptions
    {
        public static ArgumentException VersionNotDefined(string type) => new($"Version is not defined for type: {type}");
        public static InvalidOperationException Conflict(string type, string version) => new($"Type {type} with version {version} has already been registered");
        public static NotSupportedException NotSupported(string type, string version) => new($"Type {type} with version {version} is not suppoted");
        public static NotSupportedException NotSupported(Type type) => new($"Runtime type {type} is not suppoted");
    }
}
