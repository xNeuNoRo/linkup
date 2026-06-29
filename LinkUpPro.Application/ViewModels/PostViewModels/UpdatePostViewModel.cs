using System.ComponentModel.DataAnnotations;
using LinkUpPro.Application.ViewModels.Shared.ValidationAttributes;
using Microsoft.AspNetCore.Http;

namespace LinkUpPro.Application.ViewModels.PostViewModels;

/// <summary>
/// ViewModel para el formulario de edición de publicación.
/// Todos los campos son opcionales: solo se actualiza lo que se envía con valor.
/// Mantiene la información actual (CurrentImagePath, CurrentYouTubeUrl) para mostrarla en el form.
/// </summary>
public class UpdatePostViewModel
{
    [Required]
    [Display(Name = "Identificador de publicación")]
    public long PostId { get; set; }

    [NoWhitespaceOnly(ErrorMessage = "El contenido no puede contener únicamente espacios.")]
    [MaxLength(1000, ErrorMessage = "El contenido no puede exceder 1000 caracteres.")]
    [Display(Name = "Contenido")]
    public string? Content { get; set; }

    [Range(1, 2, ErrorMessage = "El tipo de contenido seleccionado no es válido.")]
    [Display(Name = "Tipo de contenido")]
    public int? ContentType { get; set; }

    [RequiredIf(nameof(ContentType), 1, ErrorMessage = "Debe seleccionar una imagen para la publicación.")]
    [ImageFileExtensions(ErrorMessage = "El archivo seleccionado no tiene un formato de imagen válido.")]
    [MaxFileSize(5, ErrorMessage = "La imagen seleccionada no puede superar los 5 MB.")]
    [MutuallyExclusiveWith(nameof(YouTubeUrl), ErrorMessage = "No debe completar ambos campos al mismo tiempo.")]
    [Display(Name = "Imagen")]
    public IFormFile? ImageFile { get; set; }

    [RequiredIf(nameof(ContentType), 2, ErrorMessage = "Debe ingresar un enlace válido de YouTube.")]
    [YouTubeUrl(ErrorMessage = "Debe ingresar un enlace válido de YouTube.")]
    [MutuallyExclusiveWith(nameof(ImageFile), ErrorMessage = "No debe completar ambos campos al mismo tiempo.")]
    [Display(Name = "Enlace de YouTube")]
    public string? YouTubeUrl { get; set; }

    [Range(1, 2, ErrorMessage = "La privacidad seleccionada no es válida.")]
    [Display(Name = "Privacidad")]
    public int? Privacy { get; set; }

    [Display(Name = "Permitir comentarios")]
    public bool? AllowComments { get; set; }

    /// <summary>
    /// Ruta actual de la imagen (solo lectura, se muestra en el form).
    /// </summary>
    [Display(Name = "Imagen actual")]
    public string? CurrentImagePath { get; set; }

    /// <summary>
    /// Enlace actual de YouTube (solo lectura, se muestra en el form).
    /// </summary>
    [Display(Name = "Enlace de YouTube actual")]
    public string? CurrentYouTubeUrl { get; set; }
}
