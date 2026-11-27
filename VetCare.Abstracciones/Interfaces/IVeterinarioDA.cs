using VetCare.Abstracciones.Comunes;
using VetCare.Abstracciones.Modelos;

namespace VetCare.Abstracciones.Interfaces;

public interface IVeterinarioDA
{
    Task<List<Veterinario>> ListarAsync(string? especialidad, bool soloActivos = false);
    Task<List<string>> ListarEspecialidadesAsync();
    Task<Veterinario?> ObtenerAsync(int id);
    Task<bool> ExisteCodigoAsync(string codigo, int? excluirVeterinarioId = null);
    Task<bool> ExisteUsuarioAsync(string usuarioId, int? excluirVeterinarioId = null);
    Task<bool> TieneCitasProgramadasAsync(int veterinarioId);
    Task<List<UsuarioOpcion>> ListarUsuariosDisponiblesAsync(int? incluirVeterinarioId = null);
    Task<Veterinario> AgregarAsync(Veterinario veterinario);
    Task<bool> ActualizarAsync(Veterinario veterinario);
    Task<bool> EliminarAsync(int id);
}
