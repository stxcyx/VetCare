using VetCare.Abstracciones.Comunes;
using VetCare.Abstracciones.Modelos;

namespace VetCare.Abstracciones.Interfaces;

public interface IProductoDA
{
    Task<PaginaResultado<Producto>> ListarAsync(string? buscar, int pagina, int tamano);
    Task<Producto?> ObtenerAsync(int id);
    Task<bool> ExisteProductoAsync(string nombre, CategoriaProducto categoria, int? excluirProductoId = null);
    Task<bool> ExisteProveedorAsync(int proveedorId);
    Task<Producto> AgregarAsync(Producto producto);
    Task<bool> ActualizarAsync(Producto producto);
    Task<bool> EliminarAsync(int id);
}