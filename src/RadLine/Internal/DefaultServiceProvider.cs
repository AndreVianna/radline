namespace RadLine.Internal;

internal sealed class DefaultServiceProvider(IServiceProvider? provider) : IServiceProvider {
    private readonly Dictionary<Type, object> _registrations = [];

    public void RegisterOptional<TService, TImplementation>(TImplementation? implementation) {
        if (implementation != null) {
            _registrations[typeof(TService)] = implementation;
        }
    }

    public object? GetService(Type serviceType) {
        if (provider != null) {
            var result = provider.GetService(serviceType);
            if (result != null) {
                return result;
            }
        }

        _registrations.TryGetValue(serviceType, out var registration);
        return registration;
    }
}
