using System.ComponentModel.DataAnnotations;
using LinkUpPro.Application.ViewModels.Shared.ValidationAttributes;

namespace LinkUpPro.Application.ViewModels.CommentViewModels;

/// <summary>
/// ViewModel para editar el contenido de un comentario o respuesta existente.
/// Solo el autor puede editarlo. No permite cambiar autor, publicación ni comentario padre.
/// </summary>
public class UpdateCommentViewModel
{
    [Required]
    [Display(Name = "Identificador de comentario")]
    public long CommentId { get; set; }

    [Required(ErrorMessage = "El contenido es requerido.")]
    [NoWhitespaceOnly(ErrorMessage = "El contenido no puede contener únicamente espacios.")]
    [MaxLength(500, ErrorMessage = "El contenido no puede exceder 500 caracteres.")]
    [Display(Name = "Contenido")]
    public string Content { get; set; } = string.Empty;
}
