using System.ComponentModel.DataAnnotations;
using LinkUpPro.Application.ViewModels.Shared.ValidationAttributes;

namespace LinkUpPro.Application.ViewModels.ProfileViewModels;

/// <summary>
/// ViewModel para el formulario de edición de "Mi Perfil".
/// ProfilePictureFile está en EditProfileViewModel (raíz) para evitar
/// problemas de model binding con IFormFile en tipos anidados.
/// </summary>
public class UpdateProfileViewModel
{
    [Required(ErrorMessage = "Debe ingresar su nombre.")]
    [NoWhitespaceOnly(ErrorMessage = "El nombre no puede contener únicamente espacios.")]
    [MaxLength(100, ErrorMessage = "El nombre no puede exceder 100 caracteres.")]
    [Display(Name = "Nombre")]
    public string FirstName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Debe ingresar su apellido.")]
    [NoWhitespaceOnly(ErrorMessage = "El apellido no puede contener únicamente espacios.")]
    [MaxLength(100, ErrorMessage = "El apellido no puede exceder 100 caracteres.")]
    [Display(Name = "Apellido")]
    public string LastName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Debe ingresar un número telefónico.")]
    [DominicanPhone]
    [Display(Name = "Teléfono")]
    public string PhoneNumber { get; set; } = string.Empty;

    /// <summary>
    /// Ruta actual de la foto (solo lectura, se muestra en el form).
    /// </summary>
    [Display(Name = "Foto actual")]
    public string? CurrentProfilePicturePath { get; set; }
}
