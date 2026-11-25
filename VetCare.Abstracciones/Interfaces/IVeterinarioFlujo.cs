using VetCare.Abstracciones.Comunes;
using VetCare.Abstracciones.Modelos;

namespace VetCare.Abstracciones.Interfaces;

public interface IVeterinarioFlujo
{
    Task<List<Veterinario>> ListarAsync(string? especialidad, bool soloActivos = false);
    Task<List<string>> ListarEspecialidadesAsync();
    Task<List<UsuarioOpcion>> ListarUsuariosDisponiblesAsync(int? incluirVeterinarioId = null);
    Task<Veterinario?> ObtenerAsync(int id);
    Task<Resultado<Veterinario>> RegistrarAsync(Veterinario veterinario);
    Task<Resultado<Veterinario>> ActualizarAsync(int id, Veterinario veterinario);
    Task<Resultado<bool>> EliminarAsync(int id);
}