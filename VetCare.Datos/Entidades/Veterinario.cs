namespace VetCare.Datos.Entidades;

public class Veterinario : EntidadBase
{
    public string Nombre { get; set; } = string.Empty;
    public string Apellido { get; set; } = string.Empty;
    public string NumeroColegiado { get; set; } = string.Empty;
    public string? Especialidad { get; set; }

    public int? UsuarioId { get; set; }
    public Usuario? Usuario { get; set; }

    public List<Cita> Citas { get; set; } = new();
    public List<Consulta> Consultas { get; set; } = new();
    public List<RegistroVacunacion> RegistrosVacunacion { get; set; } = new();
}
