namespace VetCare.Abstracciones.Modelos;

public class ResumenInventario
{
    public int ProductosActivos { get; set; }

    public int StockBajo { get; set; }

    public int Vacunas { get; set; }

    public int Medicamentos { get; set; }
}