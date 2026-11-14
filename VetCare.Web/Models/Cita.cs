using System.ComponentModel.DataAnnotations;

namespace VetCare.Web.Models;

public enum EstadoCita { Programada, Atendida, Cancelada }

public class Cita
{
    public int CitaId { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "Debe seleccionar una mascota")]
    public int MascotaId { get; set; }
    public Mascota? Mascota { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "Debe seleccionar un veterinario")]
    public int VeterinarioId { get; set; }
    public Veterinario? Veterinario { get; set; }

    public DateTime FechaHora { get; set; }
    public int DuracionMin { get; set; } = 30;

    [Required(ErrorMessage = "El motivo es obligatorio"), StringLength(200)]
    public string Motivo { get; set; } = string.Empty;

    public EstadoCita Estado { get; set; } = EstadoCita.Programada;

    [StringLength(200)]
    public string? MotivoCancelacion { get; set; }
    public string? Diagnostico { get; set; }
    public string? Tratamiento { get; set; }

    public ICollection<CitaProducto> Productos { get; set; } = new List<CitaProducto>();
}