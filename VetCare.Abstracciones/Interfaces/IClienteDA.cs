using VetCare.Abstracciones.Comunes;
using VetCare.Abstracciones.Modelos;

namespace VetCare.Abstracciones.Interfaces;

public interface IClienteDA
{
    Task<PaginaResultado<Cliente>> ListarAsync(string? buscar, int pagina, int tamano);
    Task<Cliente?> ObtenerAsync(int id, bool incluirMascotas = false);
    Task<bool> ExisteCedulaAsync(string cedula, int? excluirClienteId = null);
    Task<bool> TieneMascotasAsync(int clienteId);
    Task<Cliente> AgregarAsync(Cliente cliente);
    Task<bool> ActualizarAsync(Cliente cliente);
    Task<bool> EliminarAsync(int id);
}