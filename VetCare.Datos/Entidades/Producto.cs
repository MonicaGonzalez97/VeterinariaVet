namespace VetCare.Datos.Entidades;

public class Producto : EntidadBase
{
    public string Nombre { get; set; } = string.Empty;
    public string? Categoria { get; set; }
    public decimal Precio { get; set; }
    public int Stock { get; set; }

    public List<DetalleFactura> Detalles { get; set; } = new();
}
