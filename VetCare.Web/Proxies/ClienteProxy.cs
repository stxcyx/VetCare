using System.Net;
using System.Net.Http.Json;
using VetCare.Abstracciones.Comunes;
using VetCare.Abstracciones.Modelos;

namespace VetCare.Web.Proxies;

// Llama a la API de clientes. La Web ya no toca la base de datos para Clientes.
public class ClienteProxy
{
    private readonly HttpClient _http;
    public ClienteProxy(HttpClient http) => _http = http;

    public async Task<PaginaResultado<Cliente>> ListarAsync(string? buscar, int pagina)
    {
        var url = $"api/clientes?pagina={pagina}&buscar={Uri.EscapeDataString(buscar ?? string.Empty)}";
        return await _http.GetFromJsonAsync<PaginaResultado<Cliente>>(url) ?? new PaginaResultado<Cliente>();
    }

    public async Task<Cliente?> ObtenerAsync(int id)
    {
        var respuesta = await _http.GetAsync($"api/clientes/{id}");
        if (respuesta.StatusCode == HttpStatusCode.NotFound) return null;
        respuesta.EnsureSuccessStatusCode();
        return await respuesta.Content.ReadFromJsonAsync<Cliente>();
    }

    public async Task<Resultado<bool>> RegistrarAsync(Cliente cliente) =>
        await LeerResultadoAsync(await _http.PostAsJsonAsync("api/clientes", cliente));

    public async Task<Resultado<bool>> ActualizarAsync(int id, Cliente cliente) =>
        await LeerResultadoAsync(await _http.PutAsJsonAsync($"api/clientes/{id}", cliente));

    public async Task<Resultado<bool>> EliminarAsync(int id) =>
        await LeerResultadoAsync(await _http.DeleteAsync($"api/clientes/{id}"));

    private static async Task<Resultado<bool>> LeerResultadoAsync(HttpResponseMessage respuesta)
    {
        if (respuesta.IsSuccessStatusCode) return Resultado<bool>.Exito(true);

        if (respuesta.StatusCode is HttpStatusCode.NotFound or HttpStatusCode.Conflict)
        {
            var error = await respuesta.Content.ReadFromJsonAsync<ErrorRespuesta>()
                        ?? new ErrorRespuesta { Mensaje = "Error" };
            return respuesta.StatusCode == HttpStatusCode.NotFound
                ? Resultado<bool>.NoEncontrado(error.Mensaje)
                : Resultado<bool>.Conflicto(error.Mensaje, error.Campo);
        }

        respuesta.EnsureSuccessStatusCode();   // cualquier otro error (400, 500) se muestra como excepción
        return Resultado<bool>.Exito(true);
    }
}