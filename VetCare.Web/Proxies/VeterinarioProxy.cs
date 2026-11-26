using System.Net;
using System.Net.Http.Json;
using VetCare.Abstracciones.Comunes;
using VetCare.Abstracciones.Modelos;

namespace VetCare.Web.Proxies;

// Llama a la API de veterinarios.
public class VeterinarioProxy
{
    private readonly HttpClient _http;

    public VeterinarioProxy(HttpClient http) => _http = http;

    public async Task<List<Veterinario>> ListarAsync(string? especialidad)
    {
        var url = $"api/veterinarios?especialidad={Uri.EscapeDataString(especialidad ?? string.Empty)}";
        return await _http.GetFromJsonAsync<List<Veterinario>>(url) ?? new List<Veterinario>();
    }

    public async Task<List<string>> ListarEspecialidadesAsync() =>
        await _http.GetFromJsonAsync<List<string>>("api/veterinarios/especialidades") ?? new List<string>();

    public async Task<List<UsuarioOpcion>> ListarUsuariosAsync(int? incluirVeterinarioId = null)
    {
        var url = "api/veterinarios/usuarios";
        if (incluirVeterinarioId is not null) url += $"?incluirVeterinarioId={incluirVeterinarioId}";
        return await _http.GetFromJsonAsync<List<UsuarioOpcion>>(url) ?? new List<UsuarioOpcion>();
    }

    public async Task<Veterinario?> ObtenerAsync(int id)
    {
        var respuesta = await _http.GetAsync($"api/veterinarios/{id}");
        if (respuesta.StatusCode == HttpStatusCode.NotFound) return null;
        respuesta.EnsureSuccessStatusCode();
        return await respuesta.Content.ReadFromJsonAsync<Veterinario>();
    }

    public async Task<Resultado<bool>> RegistrarAsync(Veterinario veterinario) =>
        await LeerResultadoAsync(await _http.PostAsJsonAsync("api/veterinarios", veterinario));

    public async Task<Resultado<bool>> ActualizarAsync(int id, Veterinario veterinario) =>
        await LeerResultadoAsync(await _http.PutAsJsonAsync($"api/veterinarios/{id}", veterinario));

    public async Task<Resultado<bool>> EliminarAsync(int id) =>
        await LeerResultadoAsync(await _http.DeleteAsync($"api/veterinarios/{id}"));

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