namespace VetCare.Abstracciones.Modelos
{
    public class ProductoAtencionDetalle
    {
        public int ProductoId { get; set; }

        public string Nombre { get; set; } = string.Empty;

        public int Cantidad { get; set; }
    }
}