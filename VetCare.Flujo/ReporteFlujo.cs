using ClosedXML.Excel;
using System.Globalization;
using System.Text;
using VetCare.Abstracciones.Interfaces;
using VetCare.Abstracciones.Modelos;

namespace VetCare.Flujo
{
    public class ReporteFlujo : IReporteFlujo
    {
        private readonly IReporteDA _da;

        public ReporteFlujo(IReporteDA da)
        {
            _da = da;
        }

        public async Task<List<ReporteCita>> ObtenerReporteCitasAsync(
            FiltroReporteCita filtro)
        {
            return await _da.ObtenerReporteCitasAsync(filtro);
        }

        public async Task<ResumenReporteCitas> ObtenerResumenAsync(
    FiltroReporteCita filtro)
        {
            var citas = await _da.ObtenerReporteCitasAsync(filtro);

            var programadas = citas.Count(
                c => c.Estado == "Programada");

            var atendidas = citas.Count(
                c => c.Estado == "Atendida");

            var canceladas = citas.Count(
                c => c.Estado == "Cancelada");

            return new ResumenReporteCitas
            {
                TotalCitas = citas.Count,
                Programadas = programadas,
                Atendidas = atendidas,
                Canceladas = canceladas,

                PorEstado = new List<CitasPorEstado>
        {
            new CitasPorEstado
            {
                Estado = "Programada",
                Cantidad = programadas
            },
            new CitasPorEstado
            {
                Estado = "Atendida",
                Cantidad = atendidas
            },
            new CitasPorEstado
            {
                Estado = "Cancelada",
                Cantidad = canceladas
            }
        },

                PorVeterinario = citas
                    .GroupBy(c => c.Veterinario)
                    .Select(g => new CitasPorVeterinario
                    {
                        Veterinario = g.Key,
                        Cantidad = g.Count()
                    })
                    .OrderBy(c => c.Veterinario)
                    .ToList()
            };
        }
        public async Task<byte[]> ExportarCsvAsync(
    FiltroReporteCita filtro)
        {
            var citas = await _da.ObtenerReporteCitasAsync(filtro);

            var csv = new StringBuilder();

            csv.AppendLine(
                "ID,Fecha y hora,Cliente,Mascota,Veterinario,Motivo,Estado");

            foreach (var cita in citas)
            {
                csv.AppendLine(string.Join(",",
                    cita.CitaId,
                    EscaparCsv(cita.FechaHora.ToString(
                        "dd/MM/yyyy HH:mm",
                        CultureInfo.InvariantCulture)),
                    EscaparCsv(cita.Cliente),
                    EscaparCsv(cita.Mascota),
                    EscaparCsv(cita.Veterinario),
                    EscaparCsv(cita.Motivo),
                    EscaparCsv(cita.Estado)));
            }

            var encoding = new UTF8Encoding(true);

            return encoding.GetPreamble()
                .Concat(encoding.GetBytes(csv.ToString()))
                .ToArray();
        }
        public async Task<byte[]> ExportarExcelAsync(
            FiltroReporteCita filtro)
        {
            var citas = await _da.ObtenerReporteCitasAsync(filtro);

            using var libro = new XLWorkbook();

            var hoja = libro.Worksheets.Add("Reporte de citas");

            // Colores institucionales

            var azul = XLColor.FromHtml("#194B85");
            var azulClaro = XLColor.FromHtml("#EAF1F8");
            var gris = XLColor.FromHtml("#64748B");
            var grisClaro = XLColor.FromHtml("#F1F5F9");
            var texto = XLColor.FromHtml("#334155");
            var borde = XLColor.FromHtml("#DCE4EE");

            // Configuración general

            hoja.Style.Font.FontName = "Calibri";
            hoja.Style.Font.FontSize = 11;
            hoja.Style.Font.FontColor = texto;

            hoja.ShowGridLines = false;

            // Distribución de columnas

            hoja.Column(1).Width = 3;

            for (int columna = 2; columna <= 32; columna++)
            {
                hoja.Column(columna).Width = 4.5;
            }

            hoja.Column(33).Width = 3;

            // Encabezado institucional

            var encabezadoPrincipal =
                hoja.Range("B2:AF3");

            encabezadoPrincipal.Merge();

            hoja.Cell("B2").Value = "VetCare";

            encabezadoPrincipal.Style.Fill.BackgroundColor =
                azul;

            encabezadoPrincipal.Style.Font.FontColor =
                XLColor.White;

            encabezadoPrincipal.Style.Font.FontSize = 30;
            encabezadoPrincipal.Style.Font.Bold = true;

            Centrar(encabezadoPrincipal);

            hoja.Row(2).Height = 30;
            hoja.Row(3).Height = 30;

            // Subtítulo institucional

            var subtitulo = hoja.Range("B4:AF4");

            subtitulo.Merge();

            hoja.Cell("B4").Value =
                "SISTEMA DE GESTIÓN VETERINARIA";

            subtitulo.Style.Fill.BackgroundColor = azul;
            subtitulo.Style.Font.FontColor = XLColor.White;
            subtitulo.Style.Font.FontSize = 11;

            Centrar(subtitulo);

            hoja.Row(4).Height = 26;
            hoja.Row(5).Height = 12;

            // Título del reporte

            var titulo = hoja.Range("B6:AF7");

            titulo.Merge();

            hoja.Cell("B6").Value =
                "REPORTE DE CITAS VETERINARIAS";

            titulo.Style.Font.FontSize = 20;
            titulo.Style.Font.Bold = true;
            titulo.Style.Font.FontColor = azul;

            Centrar(titulo);

            hoja.Row(6).Height = 22;
            hoja.Row(7).Height = 22;

            // Fecha de generación

            var cultura = CultureInfo.GetCultureInfo("es-CR");

            hoja.Range("B8:AF8").Merge();

            hoja.Cell("B8").Value =
                $"Generado el {DateTime.Now.ToString(
                    "dd/MM/yyyy hh:mm tt", cultura)}";

            hoja.Range("B8:AF8").Style.Font.FontColor =
                gris;

            Centrar(hoja.Range("B8:AF8"));

            hoja.Row(8).Height = 24;
            hoja.Row(9).Height = 12;

            // Información de los filtros

            var fechaInicio = filtro.FechaInicio?
                .ToString("dd/MM/yyyy") ?? "Sin límite";

            var fechaFin = filtro.FechaFin?
                .ToString("dd/MM/yyyy") ?? "Sin límite";

            var veterinario = filtro.VeterinarioId.HasValue
                ? citas.FirstOrDefault()?.Veterinario
                    ?? $"Veterinario ID {filtro.VeterinarioId}"
                : "Todos los veterinarios";

            string periodo;

            if (!filtro.FechaInicio.HasValue &&
                !filtro.FechaFin.HasValue)
            {
                periodo = "Todas las fechas";
            }
            else if (filtro.FechaInicio.HasValue &&
                     !filtro.FechaFin.HasValue)
            {
                periodo = $"Desde el {fechaInicio}";
            }
            else if (!filtro.FechaInicio.HasValue &&
                     filtro.FechaFin.HasValue)
            {
                periodo = $"Hasta el {fechaFin}";
            }
            else
            {
                periodo = $"Del {fechaInicio} al {fechaFin}";
            }

            hoja.Range("B10:AF10").Merge();

            hoja.Cell("B10").Value =
                $"Período del reporte: {periodo}";

            hoja.Range("B11:AF11").Merge();

            hoja.Cell("B11").Value =
                $"Veterinario: {veterinario}";

            var filtros = hoja.Range("B10:AF11");

            filtros.Style.Fill.BackgroundColor = grisClaro;
            filtros.Style.Font.FontColor = texto;
            filtros.Style.Font.FontSize = 11;

            Centrar(filtros);

            hoja.Row(10).Height = 26;
            hoja.Row(11).Height = 26;
            hoja.Row(12).Height = 15;

            // Indicadores

            var total = citas.Count;

            var programadas = citas.Count(
                c => c.Estado == "Programada");

            var atendidas = citas.Count(
                c => c.Estado == "Atendida");

            var canceladas = citas.Count(
                c => c.Estado == "Cancelada");

            // Indicador 1

            CrearIndicador(
                hoja,
                "B13:H13",
                "B14:H15",
                "TOTAL DE CITAS",
                total,
                "#EDE4F8",
                "#533B78");

            // Indicador 2

            CrearIndicador(
                hoja,
                "J13:P13",
                "J14:P15",
                "PROGRAMADAS",
                programadas,
                "#DBEAFE",
                "#1D4ED8");

            // Indicador 3

            CrearIndicador(
                hoja,
                "R13:X13",
                "R14:X15",
                "ATENDIDAS",
                atendidas,
                "#DCFCE7",
                "#15803D");

            // Indicador 4

            CrearIndicador(
                hoja,
                "Z13:AF13",
                "Z14:AF15",
                "CANCELADAS",
                canceladas,
                "#FEE2E2",
                "#B91C1C");

            hoja.Row(13).Height = 25;
            hoja.Row(14).Height = 24;
            hoja.Row(15).Height = 24;
            hoja.Row(16).Height = 16;

            // Título de la tabla

            hoja.Range("B17:AF17").Merge();

            hoja.Cell("B17").Value =
                "DETALLE DE CITAS";

            hoja.Range("B17:AF17").Style.Font.FontSize =
                14;

            hoja.Range("B17:AF17").Style.Font.FontColor =
                azul;

            hoja.Range("B17:AF17").Style.Font.Bold =
                true;

            Centrar(hoja.Range("B17:AF17"));

            hoja.Row(17).Height = 30;
            hoja.Row(18).Height = 10;

            // Encabezados de la tabla

            const int filaEncabezado = 19;

            CrearCeldaTabla(
                hoja, filaEncabezado, 2, 3, "ID");

            CrearCeldaTabla(
                hoja, filaEncabezado, 4, 8, "Fecha y hora");

            CrearCeldaTabla(
                hoja, filaEncabezado, 9, 13, "Cliente");

            CrearCeldaTabla(
                hoja, filaEncabezado, 14, 17, "Mascota");

            CrearCeldaTabla(
                hoja, filaEncabezado, 18, 22, "Veterinario");

            CrearCeldaTabla(
                hoja, filaEncabezado, 23, 28, "Motivo");

            CrearCeldaTabla(
                hoja, filaEncabezado, 29, 32, "Estado");

            var encabezadoTabla =
                hoja.Range("B19:AF19");

            encabezadoTabla.Style.Fill.BackgroundColor =
                azul;

            encabezadoTabla.Style.Font.FontColor =
                XLColor.White;

            encabezadoTabla.Style.Font.Bold = true;
            encabezadoTabla.Style.Font.FontSize = 11;

            Centrar(encabezadoTabla);

            hoja.Row(filaEncabezado).Height = 32;

            // Registros

            int fila = filaEncabezado + 1;

            foreach (var cita in citas)
            {
                var registro = hoja.Range(
                    fila, 2, fila, 32);

                registro.Style.Fill.BackgroundColor =
                    fila % 2 == 0
                        ? XLColor.White
                        : azulClaro;

                registro.Style.Font.FontSize = 11;

                // ID

                CrearCeldaTabla(
                    hoja,
                    fila,
                    2,
                    3,
                    cita.CitaId);

                // Fecha y hora

                CrearCeldaTabla(
                    hoja,
                    fila,
                    4,
                    8,
                    cita.FechaHora);

                hoja.Cell(fila, 4)
                    .Style.NumberFormat.Format =
                    "dd/mm/yyyy hh:mm AM/PM";

                // Cliente

                CrearCeldaTabla(
                    hoja,
                    fila,
                    9,
                    13,
                    cita.Cliente);

                // Mascota

                CrearCeldaTabla(
                    hoja,
                    fila,
                    14,
                    17,
                    cita.Mascota);

                // Veterinario

                CrearCeldaTabla(
                    hoja,
                    fila,
                    18,
                    22,
                    cita.Veterinario);

                // Motivo

                CrearCeldaTabla(
                    hoja,
                    fila,
                    23,
                    28,
                    cita.Motivo);

                // Estado

                var estado = CrearCeldaTabla(
                    hoja,
                    fila,
                    29,
                    32,
                    cita.Estado);

                estado.Style.Font.Bold = true;

                switch (cita.Estado)
                {
                    case "Programada":

                        estado.Style.Fill.BackgroundColor =
                            XLColor.FromHtml("#DBEAFE");

                        estado.Style.Font.FontColor =
                            XLColor.FromHtml("#1D4ED8");

                        break;

                    case "Atendida":

                        estado.Style.Fill.BackgroundColor =
                            XLColor.FromHtml("#DCFCE7");

                        estado.Style.Font.FontColor =
                            XLColor.FromHtml("#15803D");

                        break;

                    case "Cancelada":

                        estado.Style.Fill.BackgroundColor =
                            XLColor.FromHtml("#FEE2E2");

                        estado.Style.Font.FontColor =
                            XLColor.FromHtml("#B91C1C");

                        break;
                }

                // Altura de la fila

                var longitud = new[]
                {
                    (cita.Cliente ?? "").Length / 22.0,
                    (cita.Mascota ?? "").Length / 17.0,
                    (cita.Veterinario ?? "").Length / 22.0,
                    (cita.Motivo ?? "").Length / 27.0
                }.Max();

                hoja.Row(fila).Height =
                    Math.Max(
                        36,
                        Math.Min(409, Math.Ceiling(longitud) * 16 + 16));

                fila++;
            }

            // Mensaje cuando no existen registros

            if (citas.Count == 0)
            {
                var mensaje = hoja.Range(
                    fila, 2, fila, 32);

                mensaje.Merge();

                mensaje.FirstCell().Value =
                    "No se encontraron citas con los filtros seleccionados.";

                mensaje.Style.Fill.BackgroundColor =
                    grisClaro;

                mensaje.Style.Font.FontColor =
                    gris;

                Centrar(mensaje);

                hoja.Row(fila).Height = 36;

                fila++;
            }

            // Total de registros

            var resumen = hoja.Range(
                fila + 1, 2, fila + 1, 32);

            resumen.Merge();

            resumen.FirstCell().Value =
                $"Total de registros: {total}";

            resumen.Style.Fill.BackgroundColor =
                grisClaro;

            resumen.Style.Font.Bold = true;
            resumen.Style.Font.FontColor = azul;

            Centrar(resumen);

            hoja.Row(fila + 1).Height = 28;

            // Pie institucional

            var pie = hoja.Range(
                fila + 3, 2, fila + 3, 32);

            pie.Merge();

            pie.FirstCell().Value =
                "VetCare | Sistema de gestión veterinaria";

            pie.Style.Fill.BackgroundColor = azul;

            pie.Style.Font.FontColor =
                XLColor.White;

            pie.Style.Font.FontSize = 10;

            Centrar(pie);

            hoja.Row(fila + 3).Height = 28;

            // Nota final

            var nota = hoja.Range(
                fila + 4, 2, fila + 4, 32);

            nota.Merge();

            nota.FirstCell().Value =
                "Reporte generado automáticamente por VetCare";

            nota.Style.Font.FontColor = gris;
            nota.Style.Font.Italic = true;
            nota.Style.Font.FontSize = 10;

            Centrar(nota);

            hoja.Row(fila + 4).Height = 24;

            // Hoja adicional para analizar los datos

            var datos = libro.Worksheets.Add("Datos de citas");

            string[] columnas =
            {
                "ID",
                "Fecha y hora",
                "Cliente",
                "Mascota",
                "Veterinario",
                "Motivo",
                "Estado"
            };

            for (int i = 0; i < columnas.Length; i++)
            {
                datos.Cell(1, i + 1).Value = columnas[i];
            }

            int filaDatos = 2;

            foreach (var cita in citas)
            {
                datos.Cell(filaDatos, 1).Value =
                    cita.CitaId;

                datos.Cell(filaDatos, 2).Value =
                    cita.FechaHora;

                datos.Cell(filaDatos, 3).Value =
                    cita.Cliente;

                datos.Cell(filaDatos, 4).Value =
                    cita.Mascota;

                datos.Cell(filaDatos, 5).Value =
                    cita.Veterinario;

                datos.Cell(filaDatos, 6).Value =
                    cita.Motivo;

                datos.Cell(filaDatos, 7).Value =
                    cita.Estado;

                datos.Cell(filaDatos, 2)
                    .Style.NumberFormat.Format =
                    "dd/mm/yyyy hh:mm";

                filaDatos++;
            }

            var encabezadoDatos =
                datos.Range("A1:G1");

            encabezadoDatos.Style.Fill.BackgroundColor =
                azul;

            encabezadoDatos.Style.Font.FontColor =
                XLColor.White;

            encabezadoDatos.Style.Font.Bold = true;

            Centrar(encabezadoDatos);

            datos.Row(1).Height = 30;

            datos.Column(1).Width = 10;
            datos.Column(2).Width = 24;
            datos.Column(3).Width = 28;
            datos.Column(4).Width = 20;
            datos.Column(5).Width = 28;
            datos.Column(6).Width = 40;
            datos.Column(7).Width = 20;

            if (citas.Count > 0)
            {
                var rangoDatos = datos.Range(
                    1, 1, filaDatos - 1, 7);

                rangoDatos.SetAutoFilter();

                var registrosDatos = datos.Range(
                    2, 1, filaDatos - 1, 7);

                registrosDatos.Style.Alignment.WrapText =
                    true;

                Centrar(registrosDatos);

                datos.Rows(2, filaDatos - 1)
                    .AdjustToContents();

                for (int i = 2; i < filaDatos; i++)
                {
                    datos.Row(i).Height =
                        Math.Max(30, datos.Row(i).Height);

                    if (i % 2 == 0)
                    {
                        datos.Range(i, 1, i, 7)
                            .Style.Fill.BackgroundColor =
                            azulClaro;
                    }
                }
            }

            datos.ShowGridLines = false;
            datos.SheetView.FreezeRows(1);

            // Configuración de impresión

            hoja.PageSetup.PageOrientation =
                XLPageOrientation.Landscape;

            hoja.PageSetup.PaperSize =
                XLPaperSize.LetterPaper;

            hoja.PageSetup.CenterHorizontally = true;

            hoja.PageSetup.FitToPages(1, 0);

            hoja.PageSetup.Margins.Top = 0.35;
            hoja.PageSetup.Margins.Bottom = 0.35;
            hoja.PageSetup.Margins.Left = 0.25;
            hoja.PageSetup.Margins.Right = 0.25;

            hoja.PageSetup.PrintAreas.Add(
                $"B2:AF{fila + 4}");

            hoja.PageSetup.SetRowsToRepeatAtTop(
                filaEncabezado,
                filaEncabezado);

            // Generar archivo Excel

            using var memoria = new MemoryStream();

            libro.SaveAs(memoria);

            return memoria.ToArray();
        }

        private static void Centrar(IXLRange rango)
        {
            rango.Style.Alignment.Horizontal =
                XLAlignmentHorizontalValues.Center;

            rango.Style.Alignment.Vertical =
                XLAlignmentVerticalValues.Center;
        }

        private static IXLRange CrearCeldaTabla(
            IXLWorksheet hoja,
            int fila,
            int columnaInicio,
            int columnaFin,
            XLCellValue valor)
        {
            var rango = hoja.Range(
                fila,
                columnaInicio,
                fila,
                columnaFin);

            rango.Merge();

            rango.FirstCell().Value = valor;

            rango.Style.Alignment.WrapText = true;

            rango.Style.Border.OutsideBorder =
                XLBorderStyleValues.Thin;

            rango.Style.Border.OutsideBorderColor =
                XLColor.FromHtml("#DCE4EE");

            Centrar(rango);

            return rango;
        }

        private static void CrearIndicador(
            IXLWorksheet hoja,
            string rangoTitulo,
            string rangoValor,
            string titulo,
            int valor,
            string colorFondo,
            string colorTexto)
        {
            var encabezado = hoja.Range(rangoTitulo);
            var contenido = hoja.Range(rangoValor);

            if (encabezado.RowCount() > 1 ||
                encabezado.ColumnCount() > 1)
            {
                encabezado.Merge();
            }

            contenido.Merge();

            encabezado.FirstCell().Value = titulo;
            contenido.FirstCell().Value = valor;

            encabezado.Style.Fill.BackgroundColor =
                XLColor.FromHtml(colorFondo);

            contenido.Style.Fill.BackgroundColor =
                XLColor.FromHtml(colorFondo);

            encabezado.Style.Font.FontColor =
                XLColor.FromHtml(colorTexto);

            contenido.Style.Font.FontColor =
                XLColor.FromHtml(colorTexto);

            encabezado.Style.Font.Bold = true;
            encabezado.Style.Font.FontSize = 10;

            contenido.Style.Font.Bold = true;
            contenido.Style.Font.FontSize = 28;

            Centrar(encabezado);
            Centrar(contenido);
        }

        private static string EscaparCsv(string? valor)
        {
            valor ??= string.Empty;

            var contenido = valor.TrimStart();

            if (contenido.StartsWith("=") ||
                contenido.StartsWith("+") ||
                contenido.StartsWith("-") ||
                contenido.StartsWith("@") ||
                valor.StartsWith("\t") ||
                valor.StartsWith("\r") ||
                valor.StartsWith("\n"))
            {
                valor = "'" + valor;
            }

            return "\"" + valor.Replace("\"", "\"\"") + "\"";
        }
    }
}