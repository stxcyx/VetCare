using Microsoft.AspNetCore.Mvc;
using VetCare.Abstracciones.Interfaces;
using VetCare.Abstracciones.Modelos;

namespace VetCare.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Produces("application/json")]
    public class ReportesController : ControllerBase
    {
        private readonly IReporteFlujo _flujo;

        public ReportesController(IReporteFlujo flujo)
        {
            _flujo = flujo;
        }

        [HttpGet("citas")]
        [ProducesResponseType(typeof(List<ReporteCita>),
            StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> ObtenerReporteCitas(
            [FromQuery] FiltroReporteCita filtro)
        {
            var resultado = await _flujo
                .ObtenerReporteCitasAsync(filtro);

            return Ok(resultado);
        }

        [HttpGet("resumen")]
        [ProducesResponseType(typeof(ResumenReporteCitas),
            StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> ObtenerResumen(
            [FromQuery] FiltroReporteCita filtro)
        {
            var resultado = await _flujo
                .ObtenerResumenAsync(filtro);

            return Ok(resultado);
        }

        [HttpGet("exportar-csv")]
        [Produces("text/csv")]
        [ProducesResponseType(typeof(FileResult),
            StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> ExportarCsv(
            [FromQuery] FiltroReporteCita filtro)
        {
            var archivo = await _flujo
                .ExportarCsvAsync(filtro);

            var nombreArchivo =
                $"ReporteCitas_{DateTime.Now:yyyyMMdd_HHmmss}.csv";

            return File(
                archivo,
                "text/csv; charset=utf-8",
                nombreArchivo);
        }

        [HttpGet("exportar-excel")]
        [Produces(
            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet")]
        [ProducesResponseType(typeof(FileResult),
            StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> ExportarExcel(
            [FromQuery] FiltroReporteCita filtro)
        {
            var archivo = await _flujo
                .ExportarExcelAsync(filtro);

            var nombreArchivo =
                $"ReporteCitas_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx";

            return File(
                archivo,
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                nombreArchivo);
        }
    }
}