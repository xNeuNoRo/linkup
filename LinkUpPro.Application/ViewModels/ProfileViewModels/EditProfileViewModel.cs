using System.ComponentModel.DataAnnotations;
using LinkUpPro.Application.ViewModels.Shared.ValidationAttributes;
using Microsoft.AspNetCore.Http;

namespace LinkUpPro.Application.ViewModels.ProfileViewModels;

/// <summary>
/// ViewModel para la pantalla de edición de "Mi Perfil".
/// Solo incluye datos personales editables y foto de perfil.
/// </summary>
public class EditProfileViewModel
{
    public UpdateProfileViewModel UpdateProfile { get; set; } = new();

    [ImageFileExtensions(ErrorMessage = "El archivo seleccionado no tiene un formato de imagen válido.")]
    [MaxFileSize(5, ErrorMessage = "La imagen seleccionada no puede superar los 5 MB.")]
    [Display(Name = "Foto de perfil")]
    public IFormFile? ProfilePictureFile { get; set; }
}
