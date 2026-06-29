namespace LinkUpPro.Tests.Base;

public static class ImplementationDiscovery
{
    private static readonly Dictionary<Type, bool> _cache = new();

    public static bool HasImplementation(Type interfaceType)
    {
        if (_cache.TryGetValue(interfaceType, out var cached))
            return cached;

        var found = AppDomain.CurrentDomain.GetAssemblies()
            .SelectMany(a =>
            {
                try { return a.GetTypes(); }
                catch { return []; }
            })
            .Any(t => interfaceType.IsAssignableFrom(t) && !t.IsInterface && !t.IsAbstract);

        _cache[interfaceType] = found;
        return found;
    }

    public static bool HasImplementation<TInterface>()
        where TInterface : class
        => HasImplementation(typeof(TInterface));

    public static Type? FindImplementation<TInterface>()
        where TInterface : class
    {
        var interfaceType = typeof(TInterface);

        return AppDomain.CurrentDomain.GetAssemblies()
            .SelectMany(a =>
            {
                try { return a.GetTypes(); }
                catch { return []; }
            })
            .FirstOrDefault(t => interfaceType.IsAssignableFrom(t) && !t.IsInterface && !t.IsAbstract);
    }
}
