using System.ComponentModel.DataAnnotations;

namespace VetCare.Negocio.Dtos;

/// <summary>Datos para crear o actualizar una vacuna del catalogo.</summary>
public class VacunaSolicitudDto
{
    /// <summary>Nombre de la vacuna. Debe ser unico.</summary>
    /// <example>Rabia</example>
    [Required(ErrorMessage = "El nombre es obligatorio")]
    [StringLength(80, MinimumLength = 2, ErrorMessage = "El nombre debe tener entre 2 y 80 caracteres")]
    public string Nombre { get; set; } = string.Empty;

    /// <summary>Dias entre una dosis y la siguiente. Con 0 no se calcula proxima dosis.</summary>
    /// <example>365</example>
    [Range(0, 3650, ErrorMessage = "El intervalo debe estar entre 0 y 3650 dias")]
    public int IntervaloDias { get; set; }
}

/// <summary>Vacuna del catalogo.</summary>
public class VacunaRespuestaDto
{
    public int Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public int IntervaloDias { get; set; }
}

/// <summary>Datos para registrar la aplicacion de una vacuna a una mascota.</summary>
public class RegistroVacunacionSolicitudDto
{
    /// <summary>Id de la mascota (activa).</summary>
    [Range(1, int.MaxValue, ErrorMessage = "Debe indicar la mascota")]
    public int MascotaId { get; set; }

    /// <summary>Id de la vacuna del catalogo.</summary>
    [Range(1, int.MaxValue, ErrorMessage = "Debe indicar la vacuna")]
    public int VacunaId { get; set; }

    /// <summary>Id del veterinario que la aplico.</summary>
    [Range(1, int.MaxValue, ErrorMessage = "Debe indicar el veterinario")]
    public int VeterinarioId { get; set; }

    /// <summary>Fecha de aplicacion (aaaa-mm-dd). No puede ser futura.</summary>
    /// <example>2026-11-08</example>
    [Required(ErrorMessage = "La fecha de aplicacion es obligatoria")]
    public DateOnly? FechaAplicacion { get; set; }

    /// <summary>Fecha de la proxima dosis (opcional). Si se omite, se calcula con el intervalo de la vacuna.</summary>
    public DateOnly? ProximaDosis { get; set; }

    /// <summary>Lote de la vacuna (opcional).</summary>
    [StringLength(40, ErrorMessage = "El lote no puede superar 40 caracteres")]
    public string? Lote { get; set; }
}

/// <summary>Vacuna aplicada a una mascota (carne de vacunacion).</summary>
public class RegistroVacunacionRespuestaDto
{
    public int Id { get; set; }
    public int MascotaId { get; set; }
    public string MascotaNombre { get; set; } = string.Empty;
    public int VacunaId { get; set; }
    public string VacunaNombre { get; set; } = string.Empty;
    public int VeterinarioId { get; set; }
    public string VeterinarioNombre { get; set; } = string.Empty;
    public DateOnly FechaAplicacion { get; set; }
    public DateOnly? ProximaDosis { get; set; }
    public string? Lote { get; set; }
}
