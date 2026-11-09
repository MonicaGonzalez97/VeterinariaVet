namespace VetCare.Datos.Entidades;

public class Especie : EntidadBase
{
    public string Nombre { get; set; } = string.Empty;

    public List<Raza> Razas { get; set; } = new();
}
