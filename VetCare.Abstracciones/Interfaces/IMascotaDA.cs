using VetCare.Abstracciones.Modelos;

namespace VetCare.Abstracciones.Interfaces;

public interface IMascotaDA
{
    Task<List<Mascota>> ListarAsync(string? buscar, int? especieId);
    Task<Mascota?> ObtenerAsync(int id, bool incluirHistorial = false);
    Task<List<Especie>> ListarEspeciesAsync();
    Task<List<Cliente>> ListarDuenosAsync();
    Task<bool> ExisteClienteAsync(int clienteId);
    Task<bool> ExisteEspecieAsync(int especieId);
    Task<bool> TieneCitasAsync(int mascotaId);
    Task<Mascota> AgregarAsync(Mascota mascota);
    Task<bool> ActualizarAsync(Mascota mascota);
    Task<bool> EliminarAsync(int id);
    Task<bool> DesactivarAsync(int id);
}