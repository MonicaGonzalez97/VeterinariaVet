using System.ComponentModel.DataAnnotations;

namespace VetCare.Negocio.Dtos;

/// <summary>Datos para registrar o actualizar una mascota.</summary>
public class MascotaSolicitudDto
{
    /// <summary>Nombre de la mascota. No se puede repetir para el mismo cliente.</summary>
    /// <example>Firulais</example>
    [Required(ErrorMessage = "El nombre es obligatorio")]
    [StringLength(60, MinimumLength = 1, ErrorMessage = "El nombre debe tener entre 1 y 60 caracteres")]
    public string Nombre { get; set; } = string.Empty;

    /// <summary>Id del cliente dueño. Debe existir.</summary>
    /// <example>1</example>
    [Range(1, int.MaxValue, ErrorMessage = "Debe indicar el cliente dueño")]
    public int ClienteId { get; set; }

    /// <summary>Id de la raza. Se consulta en GET /api/catalogos/razas.</summary>
    /// <example>1</example>
    [Range(1, int.MaxValue, ErrorMessage = "Debe indicar la raza")]
    public int RazaId { get; set; }

    /// <summary>Fecha de nacimiento (opcional), con formato aaaa-mm-dd. No puede ser futura.</summary>
    /// <example>2020-05-10</example>
    public DateOnly? FechaNacimiento { get; set; }

    /// <summary>Sexo de la mascota: Macho o Hembra.</summary>
    /// <example>Macho</example>
    [Required(ErrorMessage = "El sexo es obligatorio")]
    [RegularExpression("^(Macho|Hembra)$", ErrorMessage = "El sexo debe ser Macho o Hembra")]
    public string Sexo { get; set; } = string.Empty;

    /// <summary>
    /// Peso de la mascota (opcional) en la unidad indicada en UnidadPeso.
    /// Debe equivaler a un valor entre 0.01 kg y 200 kg (0.02 lb y 440.92 lb).
    /// </summary>
    /// <example>12.5</example>
    [Range(0.01, 1000, ErrorMessage = "El peso debe ser un numero positivo")]
    public decimal? Peso { get; set; }

    /// <summary>Unidad del peso enviado: kg (kilogramos) o lb (libras). Por defecto kg. El sistema guarda siempre en kilogramos.</summary>
    /// <example>kg</example>
    [RegularExpression("^(kg|lb)$", ErrorMessage = "La unidad de peso debe ser kg o lb")]
    public string UnidadPeso { get; set; } = "kg";

    /// <summary>Color del pelaje o plumaje (opcional).</summary>
    /// <example>Cafe</example>
    [StringLength(40, ErrorMessage = "El color no puede superar 40 caracteres")]
    public string? Color { get; set; }
}

/// <summary>Datos para registrar el fallecimiento de una mascota.</summary>
public class FallecimientoSolicitudDto
{
    /// <summary>Fecha de fallecimiento, con formato aaaa-mm-dd. No puede ser futura ni anterior al nacimiento.</summary>
    /// <example>2026-11-05</example>
    [Required(ErrorMessage = "La fecha de fallecimiento es obligatoria")]
    public DateOnly? FechaFallecimiento { get; set; }

    /// <summary>Causa del fallecimiento (opcional).</summary>
    /// <example>Vejez</example>
    [StringLength(200, ErrorMessage = "La causa no puede superar 200 caracteres")]
    public string? Causa { get; set; }

    /// <summary>Observaciones adicionales (opcional).</summary>
    [StringLength(500, ErrorMessage = "Las observaciones no pueden superar 500 caracteres")]
    public string? Observaciones { get; set; }
}

/// <summary>Registro de fallecimiento de una mascota (memorial).</summary>
public class FallecimientoRespuestaDto
{
    /// <summary>Fecha en que fallecio la mascota.</summary>
    public DateOnly FechaFallecimiento { get; set; }

    /// <summary>Causa del fallecimiento, si se registro.</summary>
    public string? Causa { get; set; }

    /// <summary>Observaciones, si se registraron.</summary>
    public string? Observaciones { get; set; }
}

/// <summary>Mascota registrada en la clinica.</summary>
public class MascotaRespuestaDto
{
    /// <summary>Identificador de la mascota.</summary>
    public int Id { get; set; }

    /// <summary>Nombre de la mascota.</summary>
    public string Nombre { get; set; } = string.Empty;

    /// <summary>Id del cliente dueño.</summary>
    public int ClienteId { get; set; }

    /// <summary>Nombre completo del cliente dueño.</summary>
    public string ClienteNombre { get; set; } = string.Empty;

    /// <summary>Id de la raza.</summary>
    public int RazaId { get; set; }

    /// <summary>Nombre de la raza.</summary>
    public string RazaNombre { get; set; } = string.Empty;

    /// <summary>Nombre de la especie.</summary>
    public string EspecieNombre { get; set; } = string.Empty;

    /// <summary>Fecha de nacimiento, si se registro.</summary>
    public DateOnly? FechaNacimiento { get; set; }

    /// <summary>Sexo: Macho o Hembra.</summary>
    public string Sexo { get; set; } = string.Empty;

    /// <summary>Peso en kilogramos, si se registro.</summary>
    public decimal? PesoKg { get; set; }

    /// <summary>Peso equivalente en libras, si se registro un peso.</summary>
    public decimal? PesoLb { get; set; }

    /// <summary>Color, si se registro.</summary>
    public string? Color { get; set; }

    /// <summary>Indica si la mascota esta fallecida (aparece en el memorial).</summary>
    public bool Fallecida { get; set; }

    /// <summary>Datos del fallecimiento. Solo tiene valor cuando Fallecida es verdadero.</summary>
    public FallecimientoRespuestaDto? Fallecimiento { get; set; }
}

/// <summary>Especie animal del catalogo.</summary>
public class EspecieDto
{
    /// <summary>Identificador de la especie.</summary>
    public int Id { get; set; }

    /// <summary>Nombre de la especie.</summary>
    public string Nombre { get; set; } = string.Empty;
}

/// <summary>Raza del catalogo, con su especie.</summary>
public class RazaDto
{
    /// <summary>Identificador de la raza.</summary>
    public int Id { get; set; }

    /// <summary>Nombre de la raza.</summary>
    public string Nombre { get; set; } = string.Empty;

    /// <summary>Id de la especie a la que pertenece.</summary>
    public int EspecieId { get; set; }

    /// <summary>Nombre de la especie a la que pertenece.</summary>
    public string EspecieNombre { get; set; } = string.Empty;
}
