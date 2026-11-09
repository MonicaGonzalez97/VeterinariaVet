namespace VetCare.Datos.Entidades;

public class Mascota : EntidadBase
{
    public string Nombre { get; set; } = string.Empty;
    public DateOnly? FechaNacimiento { get; set; }
    public string Sexo { get; set; } = string.Empty;
    public decimal? PesoKg { get; set; }
    public string? Color { get; set; }
    public bool Fallecida { get; set; }

    public int ClienteId { get; set; }
    public Cliente? Cliente { get; set; }

    public int RazaId { get; set; }
    public Raza? Raza { get; set; }

    public List<Cita> Citas { get; set; } = new();
    public List<Consulta> Consultas { get; set; } = new();
    public List<RegistroVacunacion> RegistrosVacunacion { get; set; } = new();
    public RegistroFallecimiento? RegistroFallecimiento { get; set; }
}
