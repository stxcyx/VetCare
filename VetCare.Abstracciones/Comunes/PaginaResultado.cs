namespace VetCare.Abstracciones.Comunes;

public class PaginaResultado<T>
{
    public List<T> Elementos { get; set; } = new();
    public int Pagina { get; set; } = 1;
    public int TotalPaginas { get; set; }
    public int TotalRegistros { get; set; }
}