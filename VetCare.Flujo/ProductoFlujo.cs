using VetCare.Abstracciones.Comunes;
using VetCare.Abstracciones.Interfaces;
using VetCare.Abstracciones.Modelos;

namespace VetCare.Flujo;

// Coordina cada operación: aplica las reglas y, si se cumplen, llama a DA.
public class ProductoFlujo : IProductoFlujo
{
    private const int TamanoPagina = 10;

    private readonly IProductoDA _da;
    private readonly IProductoReglas _reglas;

    public ProductoFlujo(IProductoDA da, IProductoReglas reglas)
    {
        _da = da;
        _reglas = reglas;
    }

    public Task<PaginaResultado<Producto>> ListarAsync(
        string? buscar,
        int pagina,
        CategoriaProducto? categoria = null,
        string? estado = null,
        string ordenarPor = "Nombre",
        string direccion = "ASC") =>
        _da.ListarAsync(
            buscar,
            Math.Max(1, pagina),
            TamanoPagina,
            categoria,
            estado,
            ordenarPor,
            direccion);

    public Task<Producto?> ObtenerAsync(int id) =>
        _da.ObtenerAsync(id);

    public async Task<Resultado<Producto>> RegistrarAsync(Producto producto)
    {
        var error = await _reglas.ValidarProductoUnicoAsync(
            producto.Nombre, producto.Categoria);

        if (error is not null)
            return Resultado<Producto>.Conflicto(
                error, nameof(Producto.Nombre));

        error = await _reglas.ValidarProveedorAsync(
            producto.ProveedorId);

        if (error is not null)
            return Resultado<Producto>.Conflicto(
                error, nameof(Producto.ProveedorId));

        producto.ProductoId = 0;

        return Resultado<Producto>.Exito(
            await _da.AgregarAsync(producto));
    }

    public async Task<Resultado<Producto>> ActualizarAsync(
        int id, Producto producto)
    {
        if (await _da.ObtenerAsync(id) is null)
            return Resultado<Producto>.NoEncontrado();

        var error = await _reglas.ValidarProductoUnicoAsync(
            producto.Nombre, producto.Categoria, id);

        if (error is not null)
            return Resultado<Producto>.Conflicto(
                error, nameof(Producto.Nombre));

        error = await _reglas.ValidarProveedorAsync(
            producto.ProveedorId);

        if (error is not null)
            return Resultado<Producto>.Conflicto(
                error, nameof(Producto.ProveedorId));

        producto.ProductoId = id;

        if (!await _da.ActualizarAsync(producto))
            return Resultado<Producto>.NoEncontrado();

        return Resultado<Producto>.Exito(producto);
    }

    public async Task<Resultado<bool>> EliminarAsync(int id)
    {
        if (!await _da.EliminarAsync(id))
            return Resultado<bool>.NoEncontrado();

        return Resultado<bool>.Exito(true);
    }

    public Task<List<Proveedor>> ListarProveedoresAsync() =>
        _da.ListarProveedoresAsync();

    public Task<ResumenInventario> ObtenerResumenAsync() =>
        _da.ObtenerResumenAsync();
}