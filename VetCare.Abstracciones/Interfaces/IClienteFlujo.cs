using VetCare.Abstracciones.Comunes;
using VetCare.Abstracciones.Modelos;

namespace VetCare.Abstracciones.Interfaces;

public interface IClienteFlujo
{
    Task<PaginaResultado<Cliente>> ListarAsync(string? buscar, int pagina);
    Task<Cliente?> ObtenerAsync(int id);
    Task<Resultado<Cliente>> RegistrarAsync(Cliente cliente);
    Task<Resultado<Cliente>> ActualizarAsync(int id, Cliente cliente);
    Task<Resultado<bool>> EliminarAsync(int id);
}