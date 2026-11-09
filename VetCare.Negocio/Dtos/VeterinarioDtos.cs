using System.ComponentModel.DataAnnotations;

namespace VetCare.Negocio.Dtos;

/// <summary>Datos para crear o actualizar un veterinario.</summary>
public class VeterinarioSolicitudDto
{
    /// <summary>Nombres del veterinario.</summary>
    /// <example>Laura</example>
    [Required(ErrorMessage = "El nombre es obligatorio")]
    [StringLength(80, MinimumLength = 2, ErrorMessage = "El nombre debe tener entre 2 y 80 caracteres")]
    public string Nombre { get; set; } = string.Empty;

    /// <summary>Apellidos del veterinario.</summary>
    /// <example>Ramirez</example>
    [Required(ErrorMessage = "El apellido es obligatorio")]
    [StringLength(80, MinimumLength = 2, ErrorMessage = "El apellido debe tener entre 2 y 80 caracteres")]
    public string Apellido { get; set; } = string.Empty;

    /// <summary>Numero de colegiado profesional. Debe ser unico.</summary>
    /// <example>VET-1024</example>
    [Required(ErrorMessage = "El numero de colegiado es obligatorio")]
    [StringLength(20, MinimumLength = 2, ErrorMessage = "El numero de colegiado debe tener entre 2 y 20 caracteres")]
    public string NumeroColegiado { get; set; } = string.Empty;

    /// <summary>Especialidad (opcional).</summary>
    /// <example>Medicina interna</example>
    [StringLength(80, ErrorMessage = "La especialidad no puede superar 80 caracteres")]
    public string? Especialidad { get; set; }
}

/// <summary>Veterinario de la clinica.</summary>
public class VeterinarioRespuestaDto
{
    public int Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string Apellido { get; set; } = string.Empty;

    /// <summary>Nombre y apellido juntos.</summary>
    public string NombreCompleto { get; set; } = string.Empty;

    public string NumeroColegiado { get; set; } = string.Empty;
    public string? Especialidad { get; set; }
}
