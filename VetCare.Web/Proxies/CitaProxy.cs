using System.Net;
using System.Net.Http.Json;
using VetCare.Abstracciones.Comunes;
using VetCare.Abstracciones.Modelos;

namespace VetCare.Web.Proxies;

// Llama a la API de citas.
public class CitaProxy
{
    private readonly HttpClient _http;

    public CitaProxy(HttpClient http) => _http = http;

    public async Task<List<Cita>> ListarAsync(DateTime? fecha, int? veterinarioId, EstadoCita? estado)
    {
        var parametros = new List<string>();
        if (fecha is not null) parametros.Add($"fecha={fecha.Value:yyyy-MM-dd}");
        if (veterinarioId is not null) parametros.Add($"veterinarioId={veterinarioId}");
        if (estado is not null) parametros.Add($"estado={estado}");

        var url = "api/citas" + (parametros.Count > 0 ? "?" + string.Join("&", parametros) : string.Empty);
        return await _http.GetFromJsonAsync<List<Cita>>(url) ?? new List<Cita>();
    }

    public async Task<Cita?> ObtenerAsync(int id)
    {
        var respuesta = await _http.GetAsync($"api/citas/{id}");
        if (respuesta.StatusCode == HttpStatusCode.NotFound) return null;
        respuesta.EnsureSuccessStatusCode();
        return await respuesta.Content.ReadFromJsonAsync<Cita>();
    }

    public async Task<List<Mascota>> ListarMascotasAsync() =>
        await _http.GetFromJsonAsync<List<Mascota>>("api/citas/mascotas") ?? new List<Mascota>();

    public async Task<List<Veterinario>> ListarVeterinariosAsync() =>
        await _http.GetFromJsonAsync<List<Veterinario>>("api/citas/veterinarios") ?? new List<Veterinario>();

    public async Task<int?> ObtenerVeterinarioIdPorUsuarioAsync(string usuarioId)
    {
        var respuesta = await _http.GetAsync($"api/citas/veterinario-de-usuario?usuarioId={Uri.EscapeDataString(usuarioId)}");
        if (respuesta.StatusCode == HttpStatusCode.NotFound) return null;
        respuesta.EnsureSuccessStatusCode();
        return await respuesta.Content.ReadFromJsonAsync<int>();
    }

    public async Task<Resultado<bool>> AgendarAsync(Cita cita) =>
        await LeerResultadoAsync(await _http.PostAsJsonAsync("api/citas", cita));

    public async Task<Resultado<bool>> ReprogramarAsync(int id, DateTime fechaHora) =>
        await LeerResultadoAsync(await _http.PutAsJsonAsync($"api/citas/{id}/reprogramar",
            new ReprogramarCitaDto { FechaHora = fechaHora }));

    public async Task<Resultado<bool>> CancelarAsync(int id, string motivo) =>
        await LeerResultadoAsync(await _http.PutAsJsonAsync($"api/citas/{id}/cancelar",
            new CancelarCitaDto { Motivo = motivo }));

    // HU-26: horarios libres de un veterinario en una fecha
    public async Task<Resultado<List<BloqueHorario>>> ObtenerDisponibilidadAsync(int veterinarioId, DateOnly fecha)
    {
        var respuesta = await _http.GetAsync(
            $"api/veterinarios/{veterinarioId}/disponibilidad?fecha={fecha:yyyy-MM-dd}");

        if (respuesta.IsSuccessStatusCode)
            return Resultado<List<BloqueHorario>>.Exito(
                await respuesta.Content.ReadFromJsonAsync<List<BloqueHorario>>() ?? new List<BloqueHorario>());

        var error = await respuesta.Content.ReadFromJsonAsync<ErrorRespuesta>()
                    ?? new ErrorRespuesta { Mensaje = "Error" };
        return respuesta.StatusCode == HttpStatusCode.NotFound
            ? Resultado<List<BloqueHorario>>.NoEncontrado(error.Mensaje)
            : Resultado<List<BloqueHorario>>.Conflicto(error.Mensaje, error.Campo);
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

        respuesta.EnsureSuccessStatusCode();   // cualquier otro error (400, 500) se muestra como excepción
        return Resultado<bool>.Exito(true);
    }
}
