using System.ComponentModel.DataAnnotations;

namespace VetCare.Negocio.Dtos;

/// <summary>Datos para crear o actualizar un medicamento.</summary>
public class MedicamentoSolicitudDto
{
    /// <summary>Nombre del medicamento. Debe ser unico.</summary>
    /// <example>Amoxicilina</example>
    [Required(ErrorMessage = "El nombre es obligatorio")]
    [StringLength(100, MinimumLength = 2, ErrorMessage = "El nombre debe tener entre 2 y 100 caracteres")]
    public string Nombre { get; set; } = string.Empty;

    /// <summary>Presentacion (opcional).</summary>
    /// <example>Tabletas 250 mg</example>
    [StringLength(100, ErrorMessage = "La presentacion no puede superar 100 caracteres")]
    public string? Presentacion { get; set; }

    /// <summary>Unidades disponibles.</summary>
    /// <example>40</example>
    [Range(0, 1000000, ErrorMessage = "Las existencias deben estar entre 0 y 1,000,000")]
    public int Stock { get; set; }
}

public class MedicamentoRespuestaDto
{
    public int Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string? Presentacion { get; set; }
    public int Stock { get; set; }
}

/// <summary>Datos para crear o actualizar un producto a la venta.</summary>
public class ProductoSolicitudDto
{
    /// <summary>Nombre del producto. Debe ser unico.</summary>
    /// <example>Concentrado para perro adulto 15 lb</example>
    [Required(ErrorMessage = "El nombre es obligatorio")]
    [StringLength(100, MinimumLength = 2, ErrorMessage = "El nombre debe tener entre 2 y 100 caracteres")]
    public string Nombre { get; set; } = string.Empty;

    /// <summary>Categoria (opcional).</summary>
    /// <example>Alimento</example>
    [StringLength(60, ErrorMessage = "La categoria no puede superar 60 caracteres")]
    public string? Categoria { get; set; }

    /// <summary>Precio unitario en quetzales.</summary>
    /// <example>185.50</example>
    [Range(0.01, 1000000, ErrorMessage = "El precio debe ser mayor que cero")]
    public decimal Precio { get; set; }

    /// <summary>Unidades disponibles.</summary>
    /// <example>20</example>
    [Range(0, 1000000, ErrorMessage = "Las existencias deben estar entre 0 y 1,000,000")]
    public int Stock { get; set; }
}

public class ProductoRespuestaDto
{
    public int Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string? Categoria { get; set; }
    public decimal Precio { get; set; }
    public int Stock { get; set; }
}
