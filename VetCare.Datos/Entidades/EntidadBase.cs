namespace VetCare.Datos.Entidades;

public abstract class EntidadBase
{
    public int Id { get; set; }
    public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;
}
