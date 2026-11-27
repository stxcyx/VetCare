using VetCare.Abstracciones.Comunes;
using VetCare.Abstracciones.Modelos;

namespace VetCare.Abstracciones.Interfaces;

public interface IUsuarioFlujo
{
    Task<List<UsuarioCuenta>> ListarAsync(string? rol);
    Task<UsuarioCuenta?> ObtenerAsync(string id);
    Task<Resultado<UsuarioCuenta>> CrearAsync(CrearUsuarioDto dto);
    Task<Resultado<bool>> CambiarRolAsync(string id, string rol);
    Task<Resultado<bool>> CambiarEstadoAsync(string id, bool activo);
}
