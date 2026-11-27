using VetCare.Abstracciones.Comunes;
using VetCare.Abstracciones.Interfaces;
using VetCare.Abstracciones.Modelos;

namespace VetCare.Reglas;

// Reglas de negocio de usuarios y roles. Devuelven el mensaje de error o null.
public class UsuarioReglas : IUsuarioReglas
{
    private readonly IUsuarioDA _da;

    public UsuarioReglas(IUsuarioDA da) => _da = da;

    public string? ValidarRol(string rol) =>
        RolesSistema.Todos.Contains(rol) ? null : "Seleccione un rol válido";

    public async Task<string?> ValidarCorreoUnicoAsync(string email) =>
        await _da.ExisteCorreoAsync(email) ? "Ya existe un usuario con ese correo" : null;

    public async Task<string?> ValidarCambioRolAsync(UsuarioCuenta actual, string nuevoRol)
    {
        if (actual.Rol == nuevoRol) return "El usuario ya tiene ese rol";

        // Siempre debe quedar al menos un administrador activo
        if (actual.Rol == RolesSistema.Administrador && actual.Activo
            && await _da.ContarAdministradoresActivosAsync(actual.Id) == 0)
            return "Debe existir al menos un administrador activo";

        // Un veterinario con ficha clínica necesita conservar su rol
        if (actual.Rol == RolesSistema.Veterinario && await _da.TieneVeterinarioAsync(actual.Id))
            return "No se puede cambiar el rol: la cuenta está asignada a un veterinario";

        return null;
    }

    public async Task<string?> ValidarDesactivacionAsync(UsuarioCuenta actual)
    {
        if (actual.Rol == RolesSistema.Administrador && actual.Activo
            && await _da.ContarAdministradoresActivosAsync(actual.Id) == 0)
            return "Debe existir al menos un administrador activo";

        return null;
    }
}
