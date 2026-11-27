using VetCare.Abstracciones.Modelos;

namespace VetCare.Abstracciones.Interfaces;

public interface ICitaDA
{
    // HU-17: agenda con filtros por fecha, veterinario y estado
    Task<List<Cita>> ListarAsync(DateTime? fecha, int? veterinarioId, EstadoCita? estado);
    Task<Cita?> ObtenerAsync(int id);

    // Listas desplegables del formulario
    Task<List<Mascota>> ListarMascotasActivasAsync();
    Task<List<Veterinario>> ListarVeterinariosActivosAsync();
    Task<Veterinario?> ObtenerVeterinarioAsync(int id);
    Task<bool> ExisteMascotaActivaAsync(int mascotaId);

    // RN-01: ¿el veterinario tiene otra cita Programada que se cruce con [inicio, fin)?
    Task<bool> ExisteTraslapeAsync(int veterinarioId, DateTime inicio, DateTime fin, int? excluirCitaId = null);

    // HU-17 CA-02: el veterinario que inicia sesión ve solo sus citas
    Task<int?> ObtenerVeterinarioIdPorUsuarioAsync(string usuarioId);

    Task<Cita> AgregarAsync(Cita cita);
    Task<bool> ReprogramarAsync(int id, DateTime nuevaFechaHora);
    Task<bool> CancelarAsync(int id, string motivo);
}
