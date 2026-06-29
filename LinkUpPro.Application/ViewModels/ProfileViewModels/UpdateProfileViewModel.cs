using System.ComponentModel.DataAnnotations;
using LinkUpPro.Application.ViewModels.Shared.ValidationAttributes;
using Microsoft.AspNetCore.Http;

namespace LinkUpPro.Application.ViewModels.ProfileViewModels;

/// <summary>
/// ViewModel para el formulario de edición de "Mi Perfil".
/// Campos editables: Nombre, Apellido, Teléfono, Foto de perfil.
/// Campos NO editables: UserName, Email, IsActive (se ignoran aunque vengan en el POST).
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

    [ImageFileExtensions(ErrorMessage = "El archivo seleccionado no tiene un formato de imagen válido.")]
    [MaxFileSize(5, ErrorMessage = "La imagen seleccionada no puede superar los 5 MB.")]
    [Display(Name = "Foto de perfil")]
    public IFormFile? ProfilePictureFile { get; set; }

    /// <summary>
    /// Ruta actual de la foto (solo lectura, se muestra en el form).
    /// </summary>
    [Display(Name = "Foto actual")]
    public string? CurrentProfilePicturePath { get; set; }
}
