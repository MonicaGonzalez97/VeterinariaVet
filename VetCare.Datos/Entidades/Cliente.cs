namespace VetCare.Datos.Entidades;

public class Cliente : EntidadBase
{
    public string Nombre { get; set; } = string.Empty;
    public string Apellido { get; set; } = string.Empty;
    public string Documento { get; set; } = string.Empty;
    public string Telefono { get; set; } = string.Empty;
    public string? Correo { get; set; }
    public string? Direccion { get; set; }

    public List<Mascota> Mascotas { get; set; } = new();
    public List<Factura> Facturas { get; set; } = new();
}
