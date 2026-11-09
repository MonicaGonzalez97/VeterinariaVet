namespace VetCare.Datos.Entidades;

public class Medicamento : EntidadBase
{
    public string Nombre { get; set; } = string.Empty;
    public string? Presentacion { get; set; }
    public int Stock { get; set; }

    public List<Tratamiento> Tratamientos { get; set; } = new();
}
