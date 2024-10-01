namespace RadLine.Internal.Extensions;

internal static class IServiceProviderExtensions {
    public static T? GetService<T>(this IServiceProvider provider)
        where T : class {
        ArgumentNullException.ThrowIfNull(provider);

        var result = provider.GetService(typeof(T));
        return result as T;
    }
}
