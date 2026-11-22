using VetCare.Abstracciones.Comunes;
using VetCare.Abstracciones.Modelos;

namespace VetCare.Abstracciones.Interfaces;

public interface IMascotaFlujo
{
    Task<List<Mascota>> ListarAsync(string? buscar, int? especieId);
    Task<Mascota?> ObtenerAsync(int id);
    Task<List<Especie>> ListarEspeciesAsync();
    Task<List<Cliente>> ListarDuenosAsync();
    Task<Resultado<Mascota>> RegistrarAsync(Mascota mascota);
    Task<Resultado<Mascota>> ActualizarAsync(int id, Mascota mascota);

    // Datos = true si la mascota se desactivó (tenía citas), false si se eliminó.
    Task<Resultado<bool>> EliminarAsync(int id);
}