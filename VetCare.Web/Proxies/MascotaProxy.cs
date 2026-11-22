using System.Net;
using System.Net.Http.Json;
using VetCare.Abstracciones.Comunes;
using VetCare.Abstracciones.Modelos;

namespace VetCare.Web.Proxies;

// Llama a la API de mascotas.
public class MascotaProxy
{
    private readonly HttpClient _http;
    public MascotaProxy(HttpClient http) => _http = http;

    public async Task<List<Mascota>> ListarAsync(string? buscar, int? especieId)
    {
        var url = $"api/mascotas?buscar={Uri.EscapeDataString(buscar ?? string.Empty)}";
        if (especieId is not null) url += $"&especieId={especieId}";
        return await _http.GetFromJsonAsync<List<Mascota>>(url) ?? new List<Mascota>();
    }

    public async Task<Mascota?> ObtenerAsync(int id)
    {
        var respuesta = await _http.GetAsync($"api/mascotas/{id}");
        if (respuesta.StatusCode == HttpStatusCode.NotFound) return null;
        respuesta.EnsureSuccessStatusCode();
        return await respuesta.Content.ReadFromJsonAsync<Mascota>();
    }

    public async Task<List<Especie>> ListarEspeciesAsync() =>
        await _http.GetFromJsonAsync<List<Especie>>("api/mascotas/especies") ?? new List<Especie>();

    public async Task<List<Cliente>> ListarDuenosAsync() =>
        await _http.GetFromJsonAsync<List<Cliente>>("api/mascotas/duenos") ?? new List<Cliente>();

    public async Task<Resultado<bool>> RegistrarAsync(Mascota mascota) =>
        await LeerResultadoAsync(await _http.PostAsJsonAsync("api/mascotas", mascota));

    public async Task<Resultado<bool>> ActualizarAsync(int id, Mascota mascota) =>
        await LeerResultadoAsync(await _http.PutAsJsonAsync($"api/mascotas/{id}", mascota));

    // Datos = true si la mascota quedó Inactiva, false si se eliminó
    public async Task<Resultado<bool>> EliminarAsync(int id)
    {
        var respuesta = await _http.DeleteAsync($"api/mascotas/{id}");
        if (respuesta.IsSuccessStatusCode)
            return Resultado<bool>.Exito(await respuesta.Content.ReadFromJsonAsync<bool>());
        return await LeerResultadoAsync(respuesta);
    }

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

        respuesta.EnsureSuccessStatusCode();
        return Resultado<bool>.Exito(true);
    }
}