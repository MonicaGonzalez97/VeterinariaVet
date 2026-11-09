namespace VetCare.Datos.Entidades;

public class Consulta : EntidadBase
{
    public DateTime Fecha { get; set; } = DateTime.UtcNow;
    public decimal? PesoKg { get; set; }
    public decimal? Temperatura { get; set; }
    public string? Observaciones { get; set; }

    public int? CitaId { get; set; }
    public Cita? Cita { get; set; }

    public int MascotaId { get; set; }
    public Mascota? Mascota { get; set; }

    public int VeterinarioId { get; set; }
    public Veterinario? Veterinario { get; set; }

    public List<Diagnostico> Diagnosticos { get; set; } = new();
    public List<Tratamiento> Tratamientos { get; set; } = new();
}
