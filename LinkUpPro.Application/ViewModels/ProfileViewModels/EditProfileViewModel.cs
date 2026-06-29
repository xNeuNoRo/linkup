namespace LinkUpPro.Application.ViewModels.ProfileViewModels;

/// <summary>
/// ViewModel combinado para la pantalla "Mi Perfil" (mismo formulario).
/// Incluye los datos personales editables y, opcionalmente, la sección de cambio de contraseña.
/// </summary>
public class EditProfileViewModel
{
    public UpdateProfileViewModel UpdateProfile { get; set; } = new();
    public ChangePasswordViewModel ChangePassword { get; set; } = new();
}
