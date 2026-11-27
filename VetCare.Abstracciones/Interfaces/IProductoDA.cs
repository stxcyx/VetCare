using VetCare.Abstracciones.Comunes;
using VetCare.Abstracciones.Modelos;

namespace VetCare.Abstracciones.Interfaces;

public interface IProductoDA
{
    Task<PaginaResultado<Producto>> ListarAsync(
        string? buscar,
        int pagina,
        int tamano,
        CategoriaProducto? categoria = null,
        string? estado = null,
        string ordenarPor = "Nombre",
        string direccion = "ASC");

    Task<Producto?> ObtenerAsync(int id);

    Task<bool> ExisteProductoAsync(
        string nombre,
        CategoriaProducto categoria,
        int? excluirProductoId = null);

    Task<bool> ExisteProveedorAsync(int proveedorId);

    Task<Producto> AgregarAsync(Producto producto);

    Task<bool> ActualizarAsync(Producto producto);

    Task<bool> EliminarAsync(int id);

    Task<List<Proveedor>> ListarProveedoresAsync();

    Task<ResumenInventario> ObtenerResumenAsync();
}