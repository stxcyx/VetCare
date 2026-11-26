using VetCare.Abstracciones.Comunes;
using VetCare.Abstracciones.Modelos;

namespace VetCare.Abstracciones.Interfaces;

public interface IProductoFlujo
{
    Task<PaginaResultado<Producto>> ListarAsync(string? buscar, int pagina);
    Task<Producto?> ObtenerAsync(int id);
    Task<Resultado<Producto>> RegistrarAsync(Producto producto);
    Task<Resultado<Producto>> ActualizarAsync(int id, Producto producto);
    Task<Resultado<bool>> EliminarAsync(int id);
}