using System.ComponentModel.DataAnnotations;
using LinkUpPro.Application.ViewModels.Shared.ValidationAttributes;
using Microsoft.AspNetCore.Http;

namespace LinkUpPro.Application.ViewModels.AuthViewModels;

/// <summary>
/// ViewModel para la pantalla de registro de usuario.
/// Incluye todos los campos requeridos por el documento funcional (sección "Registro de usuario").
/// </summary>
public class RegisterViewModel
{
    [Required(ErrorMessage = "El nombre es requerido.")]
    [NoWhitespaceOnly(ErrorMessage = "El nombre no puede contener únicamente espacios.")]
    [MaxLength(100, ErrorMessage = "El nombre no puede exceder 100 caracteres.")]
    [Display(Name = "Nombre")]
    public string FirstName { get; set; } = string.Empty;

    [Required(ErrorMessage = "El apellido es requerido.")]
    [NoWhitespaceOnly(ErrorMessage = "El apellido no puede contener únicamente espacios.")]
    [MaxLength(100, ErrorMessage = "El apellido no puede exceder 100 caracteres.")]
    [Display(Name = "Apellido")]
    public string LastName { get; set; } = string.Empty;

    [Required(ErrorMessage = "El teléfono es requerido.")]
    [DominicanPhone]
    [Display(Name = "Teléfono")]
    public string PhoneNumber { get; set; } = string.Empty;

    [Required(ErrorMessage = "El correo electrónico es requerido.")]
    [EmailAddress(ErrorMessage = "Debe ingresar un formato de correo electrónico válido.")]
    [MaxLength(150, ErrorMessage = "El correo electrónico no puede exceder 150 caracteres.")]
    [Display(Name = "Correo electrónico")]
    public string Email { get; set; } = string.Empty;

    [RequiredFile(ErrorMessage = "La foto de perfil es requerida.")]
    [ImageFileExtensions]
    [MaxFileSize(5, ErrorMessage = "La imagen seleccionada no puede superar los 5 MB.")]
    [Display(Name = "Foto de perfil")]
    public IFormFile? ProfilePictureFile { get; set; }

    [Required(ErrorMessage = "El nombre de usuario es requerido.")]
    [MaxLength(100, ErrorMessage = "El nombre de usuario no puede exceder 100 caracteres.")]
    [Display(Name = "Nombre de usuario")]
    public string UserName { get; set; } = string.Empty;

    [Required(ErrorMessage = "La contraseña es requerida.")]
    [DataType(DataType.Password)]
    [MinLength(8, ErrorMessage = "La contraseña debe tener al menos 8 caracteres.")]
    [RegularExpression(
        @"^(?=.*[A-Z])(?=.*[a-z])(?=.*\d)(?=.*[^a-zA-Z0-9]).{8,}$",
        ErrorMessage = "La contraseña debe tener al menos 8 caracteres, una mayúscula, una minúscula, un número y un carácter especial."
    )]
    [Display(Name = "Contraseña")]
    public string Password { get; set; } = string.Empty;

    [Required(ErrorMessage = "La confirmación de contraseña es requerida.")]
    [DataType(DataType.Password)]
    [Compare("Password", ErrorMessage = "La contraseña y su confirmación no coinciden.")]
    [Display(Name = "Confirmar contraseña")]
    public string ConfirmPassword { get; set; } = string.Empty;
}
