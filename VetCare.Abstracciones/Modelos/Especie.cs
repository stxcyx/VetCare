using System.ComponentModel.DataAnnotations;

namespace VetCare.Abstracciones.Modelos;

public class Especie
{
    public int EspecieId { get; set; }

    [Required, StringLength(40)]
    public string Nombre { get; set; } = string.Empty;

    public ICollection<Mascota> Mascotas { get; set; } = new List<Mascota>();
}