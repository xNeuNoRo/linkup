using System.ComponentModel.DataAnnotations;
using LinkUpPro.Application.ViewModels.Shared.ValidationAttributes;

namespace LinkUpPro.Application.ViewModels.CommentViewModels;

/// <summary>
/// ViewModel para crear un comentario (no respuesta) en una publicación.
/// Máximo 500 caracteres según el documento funcional.
/// </summary>
public class CreateCommentViewModel
{
    [Required(ErrorMessage = "El contenido es requerido.")]
    [NoWhitespaceOnly(ErrorMessage = "El contenido no puede contener únicamente espacios.")]
    [MaxLength(500, ErrorMessage = "El contenido no puede exceder 500 caracteres.")]
    [Display(Name = "Comentario")]
    public string Content { get; set; } = string.Empty;

    [Required]
    [Display(Name = "Identificador de publicación")]
    public long PostId { get; set; }
}
