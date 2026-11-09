namespace VetCare.Datos.Entidades;

public class Raza : EntidadBase
{
    public string Nombre { get; set; } = string.Empty;

    public int EspecieId { get; set; }
    public Especie? Especie { get; set; }

    public List<Mascota> Mascotas { get; set; } = new();
}
