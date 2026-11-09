namespace VetCare.Datos.Entidades;

public class Vacuna : EntidadBase
{
    public string Nombre { get; set; } = string.Empty;
    public int IntervaloDias { get; set; }

    public List<RegistroVacunacion> RegistrosVacunacion { get; set; } = new();
}
