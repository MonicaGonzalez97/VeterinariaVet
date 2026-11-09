namespace VetCare.Datos.Entidades;

public class Factura : EntidadBase
{
    public DateTime Fecha { get; set; } = DateTime.UtcNow;
    public decimal Total { get; set; }
    public string Estado { get; set; } = "Pendiente";

    public int ClienteId { get; set; }
    public Cliente? Cliente { get; set; }

    public List<DetalleFactura> Detalles { get; set; } = new();
}
