using System.ComponentModel.DataAnnotations;

namespace VetCare.Abstracciones.Comunes;

public static class RolesSistema
{
    public const string Administrador = "Administrador";
    public const string Recepcionista = "Recepcionista";
    public const string Veterinario = "Veterinario";

    public static readonly string[] Todos = { Administrador, Recepcionista, Veterinario };
}

public class CrearUsuarioDto
{
    [Required(ErrorMessage = "El nombre es obligatorio")]
    [StringLength(100, ErrorMessage = "Máximo 100 caracteres")]
    [Display(Name = "Nombre completo")]
    public string NombreCompleto { get; set; } = string.Empty;

    [Required(ErrorMessage = "El correo es obligatorio")]
    [EmailAddress(ErrorMessage = "Correo no válido")]
    [StringLength(256)]
    [Display(Name = "Correo")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "La contraseña es obligatoria")]
    [StringLength(100, MinimumLength = 6, ErrorMessage = "Mínimo 6 caracteres")]
    [DataType(DataType.Password)]
    [Display(Name = "Contraseña temporal")]
    public string Clave { get; set; } = string.Empty;

    [Required(ErrorMessage = "Seleccione un rol")]
    [Display(Name = "Rol")]
    public string Rol { get; set; } = string.Empty;
}

public class CambiarRolDto
{
    [Required(ErrorMessage = "Seleccione un rol")]
    [Display(Name = "Nuevo rol")]
    public string Rol { get; set; } = string.Empty;
}

public class CambiarEstadoDto
{
    public bool Activo { get; set; }
}
