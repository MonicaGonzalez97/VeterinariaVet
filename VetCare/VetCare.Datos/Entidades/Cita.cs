namespace VetCare.Datos.Entidades;

public class Cita : EntidadBase
{
    public DateTime FechaHora { get; set; }
    public string Motivo { get; set; } = string.Empty;
    public string Estado { get; set; } = "Programada";

    public int MascotaId { get; set; }
    public Mascota? Mascota { get; set; }

    public int VeterinarioId { get; set; }
    public Veterinario? Veterinario { get; set; }

    public Consulta? Consulta { get; set; }
}
