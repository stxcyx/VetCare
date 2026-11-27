using System.Net;
using System.Net.Http.Json;
using VetCare.Abstracciones.Comunes;
using VetCare.Abstracciones.Modelos;

namespace VetCare.Web.Proxies;

// Llama a la API de productos. La Web no toca la base de datos para Productos.
public class ProductoProxy : IProductoProxy
{
    private readonly HttpClient _http;
    public ProductoProxy(HttpClient http) => _http = http;

    public async Task<PaginaResultado<Producto>> ListarAsync(
        string? buscar,
        int pagina,
        CategoriaProducto? categoria = null,
        string? estado = null,
        string ordenarPor = "Nombre",
        string direccion = "ASC")
    {
        var url = $"api/productos?pagina={pagina}" +
                  $"&buscar={Uri.EscapeDataString(buscar ?? string.Empty)}" +
                  $"&ordenarPor={Uri.EscapeDataString(ordenarPor)}" +
                  $"&direccion={Uri.EscapeDataString(direccion)}";

        if (categoria.HasValue)
        {
            url += $"&categoria={Uri.EscapeDataString(categoria.Value.ToString())}";
        }

        if (!string.IsNullOrWhiteSpace(estado))
        {
            url += $"&estado={Uri.EscapeDataString(estado)}";
        }

        return await _http.GetFromJsonAsync<PaginaResultado<Producto>>(url)
            ?? new PaginaResultado<Producto>();
    }

    public async Task<Producto?> ObtenerAsync(int id)
    {
        using var respuesta = await _http.GetAsync($"api/productos/{id}");

        if (respuesta.StatusCode == HttpStatusCode.NotFound) return null;

        respuesta.EnsureSuccessStatusCode();

        return await respuesta.Content.ReadFromJsonAsync<Producto>();
    }

    public async Task<Resultado<bool>> RegistrarAsync(Producto producto) =>
        await LeerResultadoAsync(await _http.PostAsJsonAsync("api/productos", producto));

    public async Task<Resultado<bool>> ActualizarAsync(int id, Producto producto) =>
        await LeerResultadoAsync(await _http.PutAsJsonAsync($"api/productos/{id}", producto));

    public async Task<Resultado<bool>> EliminarAsync(int id) =>
        await LeerResultadoAsync(await _http.DeleteAsync($"api/productos/{id}"));

    public async Task<List<Proveedor>> ListarProveedoresAsync()
    {
        return await _http.GetFromJsonAsync<List<Proveedor>>("api/productos/proveedores")
            ?? new List<Proveedor>();
    }

    public async Task<ResumenInventario> ObtenerResumenAsync()
    {
        return await _http.GetFromJsonAsync<ResumenInventario>("api/productos/resumen")
            ?? throw new InvalidOperationException("No se recibió el resumen del inventario.");
    }

    private static async Task<Resultado<bool>> LeerResultadoAsync(HttpResponseMessage respuesta)
    {
        using (respuesta)
        {
            if (respuesta.IsSuccessStatusCode)
                return Resultado<bool>.Exito(true);

            if (respuesta.StatusCode is HttpStatusCode.NotFound or HttpStatusCode.Conflict)
            {
                var error = await respuesta.Content.ReadFromJsonAsync<ErrorRespuesta>()
                            ?? new ErrorRespuesta { Mensaje = "Error" };

                return respuesta.StatusCode == HttpStatusCode.NotFound
                    ? Resultado<bool>.NoEncontrado(error.Mensaje)
                    : Resultado<bool>.Conflicto(error.Mensaje, error.Campo);
            }

            respuesta.EnsureSuccessStatusCode();

            return Resultado<bool>.Exito(true);
        }
    }
}