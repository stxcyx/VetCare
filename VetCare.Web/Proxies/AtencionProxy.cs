using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using VetCare.Abstracciones.Modelos;

namespace VetCare.Web.Proxies
{
    public class AtencionProxy : IAtencionProxy
    {
        private readonly HttpClient _httpClient;

        public AtencionProxy(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<List<Cita>> ListarPendientesAsync()
        {
            var resultado = await _httpClient
                .GetFromJsonAsync<List<Cita>>(
                    "api/atenciones/pendientes");

            return resultado ?? new List<Cita>();
        }

        public async Task<List<Cita>> ListarHistorialAsync()
        {
            var resultado = await _httpClient
                .GetFromJsonAsync<List<Cita>>(
                    "api/atenciones/historial");

            return resultado ?? new List<Cita>();
        }

        public async Task<List<ProductoAtencionDetalle>>
            ListarProductosAsync(int citaId)
        {
            var resultado = await _httpClient
                .GetFromJsonAsync<List<ProductoAtencionDetalle>>(
                    $"api/atenciones/{citaId}/productos");

            return resultado
                ?? new List<ProductoAtencionDetalle>();
        }

        public async Task<Cita?> ObtenerAsync(int citaId)
        {
            using var respuesta = await _httpClient
                .GetAsync($"api/atenciones/{citaId}");

            if (respuesta.StatusCode == HttpStatusCode.NotFound)
            {
                return null;
            }

            respuesta.EnsureSuccessStatusCode();

            return await respuesta.Content
                .ReadFromJsonAsync<Cita>();
        }

        public async Task<string?> RegistrarAsync(
            int citaId,
            RegistroAtencion atencion)
        {
            using var respuesta = await _httpClient
                .PostAsJsonAsync(
                    $"api/atenciones/{citaId}",
                    atencion);

            if (respuesta.IsSuccessStatusCode)
            {
                return null;
            }

            if (respuesta.StatusCode == HttpStatusCode.Conflict ||
                respuesta.StatusCode == HttpStatusCode.NotFound ||
                respuesta.StatusCode == HttpStatusCode.BadRequest)
            {
                var contenido = await respuesta.Content
                    .ReadAsStringAsync();

                try
                {
                    using var json = JsonDocument.Parse(contenido);

                    if (json.RootElement.TryGetProperty(
                        "mensaje", out var mensaje))
                    {
                        return mensaje.GetString()
                            ?? "No se pudo registrar la atención.";
                    }
                }
                catch (JsonException)
                {
                    // Respuesta sin mensaje JSON válido.
                }

                return "Revise los datos de la atención.";
            }

            respuesta.EnsureSuccessStatusCode();

            return "No se pudo registrar la atención.";
        }
    }
}