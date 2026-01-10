using System.ComponentModel.DataAnnotations;

namespace SEINMX.Models.Cuenta;

public class LoginViewModel
{
    [Required(ErrorMessage = "Debe ingresar el usuario.")]
    [Display(Name = "Usuario")]
    [StringLength(50, ErrorMessage = "El usuario no puede exceder 50 caracteres.")]
    public string Usuario { get; set; } = string.Empty;
    [Required(ErrorMessage = "Debe ingresar la contraseña.")]
    [Display(Name = "Contraseña")]
    [DataType(DataType.Password)]
    [StringLength(100, MinimumLength = 6, ErrorMessage = "La contraseña debe tener al menos 6 caracteres.")]
    public string Password { get; set; } = string.Empty;
    [Display(Name = "Recordarme")]
    public bool Recuerdame { get; set; }

    public void Normalize()
    {
        Usuario = Usuario.Trim();
        Password = Password.Trim();
    }
}


public class LoginCambioViewModel
{

    [Required(ErrorMessage = "Debe ingresar la contraseña.")]
    [Display(Name = "Nueva Contraseña")]
    [DataType(DataType.Password)]
    [StringLength(100, MinimumLength = 6, ErrorMessage = "La contraseña debe tener al menos 6 caracteres.")]
    public string NuevaPassword { get; set; } = string.Empty;

    [Required(ErrorMessage = "Debe ingresar la contraseña.")]
    [Display(Name = "Confirmar Contraseña")]
    [DataType(DataType.Password)]
    [StringLength(100, MinimumLength = 6, ErrorMessage = "La contraseña debe tener al menos 6 caracteres.")]
    public string ConfirmarPassword { get; set; } = string.Empty;

    public void Normalize()
    {
        NuevaPassword = NuevaPassword.Trim();
        ConfirmarPassword = ConfirmarPassword.Trim();
    }
}
