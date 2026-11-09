namespace VetCare.Datos.Entidades;

public class RegistroVacunacion : EntidadBase
{
    public DateOnly FechaAplicacion { get; set; }
    public DateOnly? ProximaDosis { get; set; }
    public string? Lote { get; set; }

    public int MascotaId { get; set; }
    public Mascota? Mascota { get; set; }

    public int VacunaId { get; set; }
    public Vacuna? Vacuna { get; set; }

    public int VeterinarioId { get; set; }
    public Veterinario? Veterinario { get; set; }
}
