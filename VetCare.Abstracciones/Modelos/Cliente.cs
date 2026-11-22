using System.ComponentModel.DataAnnotations;


namespace VetCare.Abstracciones.Modelos;

public class Cliente
{
    public int ClienteId { get; set; }

    public string? UsuarioId { get; set; }
  

    [Required(ErrorMessage = "La cédula es obligatoria"), StringLength(15)]
    public string Cedula { get; set; } = string.Empty;

    [Required(ErrorMessage = "El nombre es obligatorio"), StringLength(60)]
    public string Nombre { get; set; } = string.Empty;

    [Required(ErrorMessage = "Los apellidos son obligatorios"), StringLength(80)]
    public string Apellidos { get; set; } = string.Empty;

    [Required(ErrorMessage = "El teléfono es obligatorio")]
    [RegularExpression(@"^\d{8}$", ErrorMessage = "El teléfono debe tener 8 dígitos")]
    public string Telefono { get; set; } = string.Empty;

    [Required(ErrorMessage = "El correo es obligatorio")]
    [EmailAddress(ErrorMessage = "El correo no tiene un formato válido")]
    public string Correo { get; set; } = string.Empty;

    [StringLength(200)]
    public string? Direccion { get; set; }

    public DateTime FechaRegistro { get; set; } = DateTime.Now;

    public ICollection<Mascota> Mascotas { get; set; } = new List<Mascota>();
}