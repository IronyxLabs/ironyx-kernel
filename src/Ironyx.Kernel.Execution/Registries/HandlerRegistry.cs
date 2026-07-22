namespace Ironyx.Kernel.Execution.Registries
{
    public class HandlerRegistry : IHandlerRegistry, IHandlerTypeResolver
    {
        private readonly Dictionary<Type, Type> _registrations = [];

        public Type this[Type type]
        {
            get
            {
                if (!_registrations.TryGetValue(type, out var result)) throw new InvalidOperationException($"Handler has not been registered for type: {type}");

                return result!;
            }
        }

        public void Add(Type command, Type handler)
        {
            if (_registrations.ContainsKey(command)) throw new InvalidOperationException($"Handler has already been registered for type: {command}");

            _registrations.Add(command, handler);
        }
    }
}
