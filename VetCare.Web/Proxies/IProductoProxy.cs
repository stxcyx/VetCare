using VetCare.Abstracciones.Comunes;
using VetCare.Abstracciones.Modelos;

namespace VetCare.Web.Proxies;

public interface IProductoProxy
{
    Task<PaginaResultado<Producto>> ListarAsync(
        string? buscar,
        int pagina,
        CategoriaProducto? categoria = null,
        string? estado = null,
        string ordenarPor = "Nombre",
        string direccion = "ASC");

    Task<Producto?> ObtenerAsync(int id);

    Task<Resultado<bool>> RegistrarAsync(Producto producto);

    Task<Resultado<bool>> ActualizarAsync(int id, Producto producto);

    Task<Resultado<bool>> EliminarAsync(int id);

    Task<List<Proveedor>> ListarProveedoresAsync();

    Task<ResumenInventario> ObtenerResumenAsync();
}