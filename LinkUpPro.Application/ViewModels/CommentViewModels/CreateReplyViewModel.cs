using System.ComponentModel.DataAnnotations;
using LinkUpPro.Application.ViewModels.Shared.ValidationAttributes;

namespace LinkUpPro.Application.ViewModels.CommentViewModels;

/// <summary>
/// ViewModel para crear una respuesta anidada a un comentario existente.
/// </summary>
public class CreateReplyViewModel
{
    [Required(ErrorMessage = "El contenido es requerido.")]
    [NoWhitespaceOnly(ErrorMessage = "El contenido no puede contener únicamente espacios.")]
    [MaxLength(500, ErrorMessage = "El contenido no puede exceder 500 caracteres.")]
    [Display(Name = "Respuesta")]
    public string Content { get; set; } = string.Empty;

    [Required]
    [Display(Name = "Identificador de publicación")]
    public long PostId { get; set; }

    [Required]
    [Display(Name = "Identificador de comentario padre")]
    public long ParentCommentId { get; set; }
}
