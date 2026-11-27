namespace VetCare.Abstracciones.Modelos;

// Cuenta de acceso al sistema con su rol y estado (HU de Usuarios y roles)
public class UsuarioCuenta
{
    public string Id { get; set; } = string.Empty;
    public string NombreCompleto { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Rol { get; set; } = string.Empty;
    public bool Activo { get; set; }
}
