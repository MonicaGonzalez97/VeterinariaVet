using System.ComponentModel.DataAnnotations;

namespace VetCare.Negocio.Dtos;

/// <summary>Linea de una factura: un producto del inventario o un concepto libre (por ejemplo, una consulta).</summary>
public class DetalleFacturaSolicitudDto
{
    /// <summary>Id del producto (opcional). Si se indica, se toma su nombre y precio y se descuenta de las existencias.</summary>
    public int? ProductoId { get; set; }

    /// <summary>Descripcion. Obligatoria cuando no se indica un producto.</summary>
    /// <example>Consulta veterinaria</example>
    [StringLength(200, ErrorMessage = "La descripcion no puede superar 200 caracteres")]
    public string? Descripcion { get; set; }

    /// <summary>Cantidad de unidades.</summary>
    /// <example>1</example>
    [Range(1, 1000, ErrorMessage = "La cantidad debe estar entre 1 y 1000")]
    public int Cantidad { get; set; } = 1;

    /// <summary>Precio unitario. Obligatorio cuando no se indica un producto.</summary>
    /// <example>150.00</example>
    [Range(0.01, 1000000, ErrorMessage = "El precio debe ser mayor que cero")]
    public decimal? PrecioUnitario { get; set; }
}

/// <summary>Datos para emitir una factura.</summary>
public class FacturaSolicitudDto
{
    /// <summary>Id del cliente a quien se factura.</summary>
    [Range(1, int.MaxValue, ErrorMessage = "Debe indicar el cliente")]
    public int ClienteId { get; set; }

    /// <summary>Al menos una linea.</summary>
    [Required(ErrorMessage = "Debe incluir al menos una linea")]
    [MinLength(1, ErrorMessage = "Debe incluir al menos una linea")]
    public List<DetalleFacturaSolicitudDto> Detalles { get; set; } = new();
}

public class DetalleFacturaRespuestaDto
{
    public int Id { get; set; }
    public int? ProductoId { get; set; }
    public string Descripcion { get; set; } = string.Empty;
    public int Cantidad { get; set; }
    public decimal PrecioUnitario { get; set; }
    public decimal Subtotal { get; set; }
}

/// <summary>Factura emitida.</summary>
public class FacturaRespuestaDto
{
    public int Id { get; set; }
    public DateTime Fecha { get; set; }
    public int ClienteId { get; set; }
    public string ClienteNombre { get; set; } = string.Empty;
    public decimal Total { get; set; }

    /// <summary>Pendiente, Pagada o Anulada.</summary>
    public string Estado { get; set; } = string.Empty;

    public List<DetalleFacturaRespuestaDto> Detalles { get; set; } = new();
}
