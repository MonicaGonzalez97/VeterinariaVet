namespace VetCare.Datos.Entidades;

public class DetalleFactura : EntidadBase
{
    public string Descripcion { get; set; } = string.Empty;
    public int Cantidad { get; set; }
    public decimal PrecioUnitario { get; set; }

    public int FacturaId { get; set; }
    public Factura? Factura { get; set; }

    public int? ProductoId { get; set; }
    public Producto? Producto { get; set; }
}
