namespace LinkUpPro.Application.ViewModels.Shared;

/// <summary>
/// Modelo de vista estándar para representar errores globales no controlados
/// o excepciones de negocio que escapan hacia la capa de presentación.
/// </summary>
public class ErrorViewModel
{
    /// <summary>
    /// Identificador único de la petición (Request ID) para facilitar la trazabilidad en logs.
    /// </summary>
    public string? RequestId { get; set; }

    /// <summary>
    /// Define si se debe mostrar el RequestId en la interfaz de usuario.
    /// </summary>
    public bool ShowRequestId => !string.IsNullOrEmpty(RequestId);

    /// <summary>
    /// Mensaje amigable para el usuario sobre lo ocurrido.
    /// </summary>
    public string ErrorMessage { get; set; } =
        "Ha ocurrido un error inesperado al procesar su solicitud.";

    /// <summary>
    /// (Opcional) Código de estado HTTP (Ej. 404, 500) para contexto visual.
    /// </summary>
    public int StatusCode { get; set; } = 500;

    /// <summary>
    /// Título corto del error (Ej. "Acceso Denegado", "No Encontrado").
    /// </summary>
    public string ErrorTitle { get; set; } = "Error del Sistema";
}
