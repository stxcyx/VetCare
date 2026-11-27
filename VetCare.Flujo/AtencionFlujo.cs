using VetCare.Abstracciones.Interfaces;
using VetCare.Abstracciones.Modelos;

namespace VetCare.Flujo
{
    public class AtencionFlujo : IAtencionFlujo
    {
        private readonly IAtencionDA _da;

        public AtencionFlujo(IAtencionDA da)
        {
            _da = da;
        }

        public async Task<List<Cita>> ListarPendientesAsync()
        {
            return await _da.ListarPendientesAsync();
        }

        public async Task<List<Cita>> ListarHistorialAsync()
        {
            return await _da.ListarHistorialAsync();
        }

        public async Task<List<ProductoAtencionDetalle>>
            ListarProductosAsync(int citaId)
        {
            return await _da.ListarProductosAsync(citaId);
        }

        public async Task<Cita?> ObtenerAsync(int citaId)
        {
            return await _da.ObtenerAsync(citaId);
        }

        public async Task<string?> RegistrarAsync(
            int citaId,
            RegistroAtencion atencion)
        {
            return await _da.RegistrarAsync(
                citaId,
                atencion);
        }
    }
}