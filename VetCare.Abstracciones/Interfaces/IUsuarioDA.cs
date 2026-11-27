using VetCare.Abstracciones.Comunes;
using VetCare.Abstracciones.Modelos;

namespace VetCare.Abstracciones.Interfaces;

public interface IUsuarioDA
{
    Task<List<UsuarioCuenta>> ListarAsync(string? rol);
    Task<UsuarioCuenta?> ObtenerAsync(string id);
    Task<bool> ExisteCorreoAsync(string email);
    Task<int> ContarAdministradoresActivosAsync(string? excluirUsuarioId = null);
    Task<bool> TieneVeterinarioAsync(string usuarioId);
    Task<(string? Error, string? Id)> CrearAsync(CrearUsuarioDto dto);
    Task<string?> CambiarRolAsync(string id, string rol);
    Task<string?> CambiarEstadoAsync(string id, bool activo);
}
