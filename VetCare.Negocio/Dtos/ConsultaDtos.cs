using System.ComponentModel.DataAnnotations;

namespace VetCare.Negocio.Dtos;

/// <summary>Diagnostico emitido en una consulta.</summary>
public class DiagnosticoSolicitudDto
{
    /// <summary>Descripcion del diagnostico.</summary>
    /// <example>Otitis externa</example>
    [Required(ErrorMessage = "La descripcion del diagnostico es obligatoria")]
    [StringLength(300, MinimumLength = 3, ErrorMessage = "La descripcion debe tener entre 3 y 300 caracteres")]
    public string Descripcion { get; set; } = string.Empty;

    /// <summary>Gravedad: Leve, Moderada o Grave (opcional).</summary>
    /// <example>Leve</example>
    [RegularExpression("^(Leve|Moderada|Grave)$", ErrorMessage = "La gravedad debe ser Leve, Moderada o Grave")]
    public string? Gravedad { get; set; }
}

/// <summary>Tratamiento indicado en una consulta.</summary>
public class TratamientoSolicitudDto
{
    /// <summary>Id del medicamento (opcional). Descuenta una unidad de existencias.</summary>
    public int? MedicamentoId { get; set; }

    /// <summary>Indicaciones para el cliente.</summary>
    /// <example>Una tableta cada 12 horas con alimento</example>
    [Required(ErrorMessage = "Las indicaciones son obligatorias")]
    [StringLength(300, MinimumLength = 3, ErrorMessage = "Las indicaciones deben tener entre 3 y 300 caracteres")]
    public string Indicaciones { get; set; } = string.Empty;

    /// <summary>Dosis (opcional).</summary>
    [StringLength(80, ErrorMessage = "La dosis no puede superar 80 caracteres")]
    public string? Dosis { get; set; }

    /// <summary>Duracion del tratamiento en dias.</summary>
    /// <example>7</example>
    [Range(1, 365, ErrorMessage = "La duracion debe estar entre 1 y 365 dias")]
    public int DuracionDias { get; set; } = 1;
}

/// <summary>Datos para registrar una consulta medica con sus diagnosticos y tratamientos.</summary>
public class ConsultaSolicitudDto
{
    /// <summary>Id de la mascota (activa).</summary>
    [Range(1, int.MaxValue, ErrorMessage = "Debe indicar la mascota")]
    public int MascotaId { get; set; }

    /// <summary>Id del veterinario que atiende.</summary>
    [Range(1, int.MaxValue, ErrorMessage = "Debe indicar el veterinario")]
    public int VeterinarioId { get; set; }

    /// <summary>Id de la cita que se atiende (opcional). Debe estar programada y ser de la misma mascota y veterinario; pasa a Completada.</summary>
    public int? CitaId { get; set; }

    /// <summary>Peso en kilogramos (opcional). Tambien actualiza el peso de la mascota.</summary>
    /// <example>12.5</example>
    [Range(0.01, 1000, ErrorMessage = "El peso debe ser un numero positivo")]
    public decimal? PesoKg { get; set; }

    /// <summary>Temperatura en grados Celsius (opcional), entre 30 y 45.</summary>
    /// <example>38.5</example>
    public decimal? Temperatura { get; set; }

    /// <summary>Observaciones del veterinario (opcional).</summary>
    [StringLength(1000, ErrorMessage = "Las observaciones no pueden superar 1000 caracteres")]
    public string? Observaciones { get; set; }

    /// <summary>Al menos un diagnostico.</summary>
    [Required(ErrorMessage = "Debe incluir al menos un diagnostico")]
    [MinLength(1, ErrorMessage = "Debe incluir al menos un diagnostico")]
    public List<DiagnosticoSolicitudDto> Diagnosticos { get; set; } = new();

    /// <summary>Tratamientos indicados (puede estar vacio).</summary>
    public List<TratamientoSolicitudDto> Tratamientos { get; set; } = new();
}

public class DiagnosticoRespuestaDto
{
    public int Id { get; set; }
    public string Descripcion { get; set; } = string.Empty;
    public string? Gravedad { get; set; }
}

public class TratamientoRespuestaDto
{
    public int Id { get; set; }
    public int? MedicamentoId { get; set; }
    public string? MedicamentoNombre { get; set; }
    public string Indicaciones { get; set; } = string.Empty;
    public string? Dosis { get; set; }
    public int DuracionDias { get; set; }
}

/// <summary>Consulta medica registrada.</summary>
public class ConsultaRespuestaDto
{
    public int Id { get; set; }
    public DateTime Fecha { get; set; }
    public int MascotaId { get; set; }
    public string MascotaNombre { get; set; } = string.Empty;
    public int VeterinarioId { get; set; }
    public string VeterinarioNombre { get; set; } = string.Empty;
    public int? CitaId { get; set; }
    public decimal? PesoKg { get; set; }
    public decimal? Temperatura { get; set; }
    public string? Observaciones { get; set; }
    public List<DiagnosticoRespuestaDto> Diagnosticos { get; set; } = new();
    public List<TratamientoRespuestaDto> Tratamientos { get; set; } = new();
}
