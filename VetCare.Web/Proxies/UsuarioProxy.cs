using System.Net;
using System.Net.Http.Json;
using VetCare.Abstracciones.Comunes;
using VetCare.Abstracciones.Modelos;

namespace VetCare.Web.Proxies;

// Llama a la API de usuarios.
public class UsuarioProxy
{
    private readonly HttpClient _http;

    public UsuarioProxy(HttpClient http) => _http = http;

    public async Task<List<UsuarioCuenta>> ListarAsync(string? rol)
    {
        var url = $"api/usuarios?rol={Uri.EscapeDataString(rol ?? string.Empty)}";
        return await _http.GetFromJsonAsync<List<UsuarioCuenta>>(url) ?? new List<UsuarioCuenta>();
    }

    public async Task<UsuarioCuenta?> ObtenerAsync(string id)
    {
        var respuesta = await _http.GetAsync($"api/usuarios/{Uri.EscapeDataString(id)}");
        if (respuesta.StatusCode == HttpStatusCode.NotFound) return null;
        respuesta.EnsureSuccessStatusCode();
        return await respuesta.Content.ReadFromJsonAsync<UsuarioCuenta>();
    }

    public async Task<Resultado<bool>> CrearAsync(CrearUsuarioDto dto) =>
        await LeerResultadoAsync(await _http.PostAsJsonAsync("api/usuarios", dto));

    public async Task<Resultado<bool>> CambiarRolAsync(string id, string rol) =>
        await LeerResultadoAsync(await _http.PutAsJsonAsync(
            $"api/usuarios/{Uri.EscapeDataString(id)}/rol", new CambiarRolDto { Rol = rol }));

    public async Task<Resultado<bool>> CambiarEstadoAsync(string id, bool activo) =>
        await LeerResultadoAsync(await _http.PutAsJsonAsync(
            $"api/usuarios/{Uri.EscapeDataString(id)}/estado", new CambiarEstadoDto { Activo = activo }));

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
