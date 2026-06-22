using Xunit;

namespace LinkUpPro.Tests.Base;

public sealed class ServiceFactAttribute : FactAttribute
{
    private readonly Type _interfaceType;
    private readonly Lazy<bool> _hasImplementation;

    public ServiceFactAttribute(Type serviceInterfaceType)
    {
        _interfaceType = serviceInterfaceType;
        _hasImplementation = new Lazy<bool>(() => ImplementationDiscovery.HasImplementation(serviceInterfaceType));
    }

    public override string? Skip
    {
        get => _hasImplementation.Value
            ? null
            : $"No se encontro implementacion concreta de {_interfaceType.Name}. Cree una clase que implemente esta interfaz para ejecutar estos tests.";
        set { }
    }
}
