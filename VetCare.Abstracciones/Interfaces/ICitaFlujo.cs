using VetCare.Abstracciones.Comunes;
using VetCare.Abstracciones.Modelos;

namespace VetCare.Abstracciones.Interfaces;

public interface ICitaFlujo
{
    Task<List<Cita>> ListarAsync(DateTime? fecha, int? veterinarioId, EstadoCita? estado);
    Task<Cita?> ObtenerAsync(int id);
    Task<List<Mascota>> ListarMascotasActivasAsync();
    Task<List<Veterinario>> ListarVeterinariosActivosAsync();
    Task<int?> ObtenerVeterinarioIdPorUsuarioAsync(string usuarioId);

    Task<Resultado<Cita>> AgendarAsync(Cita cita);
    Task<Resultado<Cita>> ReprogramarAsync(int id, DateTime nuevaFechaHora);
    Task<Resultado<bool>> CancelarAsync(int id, string motivo);
}
