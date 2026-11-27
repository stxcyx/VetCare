using VetCare.Abstracciones.Modelos;

namespace VetCare.Abstracciones.Interfaces;

// Cada método devuelve null si la regla se cumple o el mensaje de error si no.
public interface IProductoReglas
{
    Task<string?> ValidarProductoUnicoAsync(string nombre, CategoriaProducto categoria, int? productoId = null);
    Task<string?> ValidarProveedorAsync(int proveedorId);
}