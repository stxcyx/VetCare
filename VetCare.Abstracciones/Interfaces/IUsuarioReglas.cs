using VetCare.Abstracciones.Modelos;

namespace VetCare.Abstracciones.Interfaces;

// Cada método devuelve el mensaje de error o null si la regla se cumple
public interface IUsuarioReglas
{
    string? ValidarRol(string rol);
    Task<string?> ValidarCorreoUnicoAsync(string email);
    Task<string?> ValidarCambioRolAsync(UsuarioCuenta actual, string nuevoRol);
    Task<string?> ValidarDesactivacionAsync(UsuarioCuenta actual);
}
