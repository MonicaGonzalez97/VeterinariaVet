using System.ComponentModel.DataAnnotations;

namespace VetCare.Negocio.Dtos;

/// <summary>Datos para programar o reprogramar una cita.</summary>
public class CitaSolicitudDto
{
    /// <summary>Id de la mascota (activa).</summary>
    [Range(1, int.MaxValue, ErrorMessage = "Debe indicar la mascota")]
    public int MascotaId { get; set; }

    /// <summary>Id del veterinario que atendera.</summary>
    [Range(1, int.MaxValue, ErrorMessage = "Debe indicar el veterinario")]
    public int VeterinarioId { get; set; }

    /// <summary>Fecha y hora de la cita (aaaa-mm-ddThh:mm), en hora de la clinica. No puede ser de un dia anterior a hoy.</summary>
    /// <example>2026-11-12T09:30:00</example>
    [Required(ErrorMessage = "La fecha y hora son obligatorias")]
    public DateTime? FechaHora { get; set; }

    /// <summary>Motivo de la cita.</summary>
    /// <example>Vacunacion anual</example>
    [Required(ErrorMessage = "El motivo es obligatorio")]
    [StringLength(200, MinimumLength = 3, ErrorMessage = "El motivo debe tener entre 3 y 200 caracteres")]
    public string Motivo { get; set; } = string.Empty;
}

/// <summary>Cita de la agenda.</summary>
public class CitaRespuestaDto
{
    public int Id { get; set; }
    public int MascotaId { get; set; }
    public string MascotaNombre { get; set; } = string.Empty;
    public string ClienteNombre { get; set; } = string.Empty;
    public int VeterinarioId { get; set; }
    public string VeterinarioNombre { get; set; } = string.Empty;
    public DateTime FechaHora { get; set; }
    public string Motivo { get; set; } = string.Empty;

    /// <summary>Programada, Completada o Cancelada.</summary>
    public string Estado { get; set; } = string.Empty;
}
