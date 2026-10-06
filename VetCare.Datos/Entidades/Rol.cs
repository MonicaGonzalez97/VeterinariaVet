namespace VetCare.Datos.Entidades;

public class Rol : EntidadBase
{
    public string Nombre { get; set; } = string.Empty;
    public string? Descripcion { get; set; }

    public List<Usuario> Usuarios { get; set; } = new();
}
