namespace LinkUpPro.Domain.Common;

/// <summary>
/// Proveedor de fecha y hora UTC para que la capa de infraestructura centralice
/// el acceso al reloj del sistema. Esto nos facilitara las pruebas unitarias
/// al permitir simular la fecha y hora actual.
/// </summary>
public interface IDateTimeProvider
{
    /// <summary>
    /// Obtiene la fecha y hora actual en formato UTC con offset de zona horaria.
    /// </summary>
    DateTimeOffset UtcNow { get; }
}
