using System.ComponentModel.DataAnnotations;
using LinkUpPro.Application.ViewModels.Shared.ValidationAttributes;
using LinkUpPro.Domain.Enums;
using Microsoft.AspNetCore.Http;

namespace LinkUpPro.Application.ViewModels.PostViewModels;

/// <summary>
/// ViewModel para el formulario de creación de publicación (Home, parte superior).
/// El campo Imagen y YouTube son mutuamente excluyentes: solo uno de los dos puede tener valor.
/// La validación de exclusividad se hace en el servicio del backend o en el controller.
/// </summary>
public class CreatePostViewModel
{
    [Required(ErrorMessage = "Debe ingresar el contenido de la publicación.")]
    [NoWhitespaceOnly(ErrorMessage = "El contenido no puede contener únicamente espacios.")]
    [MaxLength(1000, ErrorMessage = "El contenido no puede exceder 1000 caracteres.")]
    [Display(Name = "Contenido")]
    public string Content { get; set; } = string.Empty;

    /// <summary>
    /// Tipo de contenido: 1 = Imagen, 2 = YouTube. Mapea a PostContentType.
    /// </summary>
    [Required(ErrorMessage = "Debe seleccionar el tipo de contenido.")]
    [Range(1, 2, ErrorMessage = "El tipo de contenido seleccionado no es válido.")]
    [Display(Name = "Tipo de contenido")]
    public int ContentType { get; set; } = (int)PostContentType.Image;

    /// <summary>
    /// Archivo de imagen (opcional si ContentType = 2, requerido si ContentType = 1).
    /// </summary>
    [RequiredIf(nameof(ContentType), 1, ErrorMessage = "Debe seleccionar una imagen para crear la publicación.")]
    [ImageFileExtensions(ErrorMessage = "El archivo seleccionado no tiene un formato de imagen válido.")]
    [MaxFileSize(5, ErrorMessage = "La imagen seleccionada no puede superar los 5 MB.")]
    [MutuallyExclusiveWith(nameof(YouTubeUrl), ErrorMessage = "No debe completar ambos campos al mismo tiempo.")]
    [Display(Name = "Imagen")]
    public IFormFile? ImageFile { get; set; }

    /// <summary>
    /// URL de YouTube (opcional si ContentType = 1, requerido si ContentType = 2).
    /// </summary>
    [RequiredIf(nameof(ContentType), 2, ErrorMessage = "Debe ingresar un enlace válido de YouTube.")]
    [YouTubeUrl(ErrorMessage = "Debe ingresar un enlace válido de YouTube.")]
    [MutuallyExclusiveWith(nameof(ImageFile), ErrorMessage = "No debe completar ambos campos al mismo tiempo.")]
    [Display(Name = "Enlace de YouTube")]
    public string? YouTubeUrl { get; set; }

    /// <summary>
    /// Privacidad: 1 = Solo amigos (default), 2 = Solo yo. Mapea a PrivacyLevel.
    /// </summary>
    [Required(ErrorMessage = "Debe seleccionar la privacidad.")]
    [Range(1, 2, ErrorMessage = "La privacidad seleccionada no es válida.")]
    [Display(Name = "Privacidad")]
    public int Privacy { get; set; } = (int)PrivacyLevel.FriendsOnly;

    [Display(Name = "Permitir comentarios")]
    public bool AllowComments { get; set; } = true;
}
