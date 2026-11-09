namespace VetCare.Datos.Entidades;

public class RegistroFallecimiento : EntidadBase
{
    public DateOnly FechaFallecimiento { get; set; }
    public string? Causa { get; set; }
    public string? Observaciones { get; set; }

    public int MascotaId { get; set; }
    public Mascota? Mascota { get; set; }

    public int? UsuarioRegistroId { get; set; }
    public Usuario? UsuarioRegistro { get; set; }
}
