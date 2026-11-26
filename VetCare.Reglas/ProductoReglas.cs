using VetCare.Abstracciones.Interfaces;
using VetCare.Abstracciones.Modelos;

namespace VetCare.Reglas;

public class ProductoReglas : IProductoReglas
{
    public const string MsgProductoDuplicado = "Ya existe un producto registrado con este nombre y categoría";
    public const string MsgProveedorNoExiste = "El proveedor seleccionado no existe";

    private readonly IProductoDA _da;
    public ProductoReglas(IProductoDA da) => _da = da;

    // HU-18 y HU-19: el producto debe ser único por nombre y categoría
    public async Task<string?> ValidarProductoUnicoAsync(string nombre, CategoriaProducto categoria, int? productoId = null) =>
        await _da.ExisteProductoAsync(nombre, categoria, productoId) ? MsgProductoDuplicado : null;

    // HU-18: el proveedor debe existir
    public async Task<string?> ValidarProveedorAsync(int proveedorId) =>
        await _da.ExisteProveedorAsync(proveedorId) ? null : MsgProveedorNoExiste;
}