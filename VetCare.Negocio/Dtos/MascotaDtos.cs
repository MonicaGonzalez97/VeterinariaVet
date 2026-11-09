using System.ComponentModel.DataAnnotations;

namespace VetCare.Negocio.Dtos;

public class MascotaSolicitudDto
{
    [Required(ErrorMessage = "El nombre es obligatorio")]
    [StringLength(60, MinimumLength = 1, ErrorMessage = "El nombre debe tener entre 1 y 60 caracteres")]
    public string Nombre { get; set; } = string.Empty;

    [Range(1, int.MaxValue, ErrorMessage = "Debe indicar el cliente dueño")]
    public int ClienteId { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "Debe indicar la raza")]
    public int RazaId { get; set; }

    public DateOnly? FechaNacimiento { get; set; }

    [Required(ErrorMessage = "El sexo es obligatorio")]
    [RegularExpression("^(Macho|Hembra)$", ErrorMessage = "El sexo debe ser Macho o Hembra")]
    public string Sexo { get; set; } = string.Empty;

    [Range(0.01, 200, ErrorMessage = "El peso debe estar entre 0.01 y 200 kg")]
    public decimal? PesoKg { get; set; }

    [StringLength(40, ErrorMessage = "El color no puede superar 40 caracteres")]
    public string? Color { get; set; }
}

public class FallecimientoSolicitudDto
{
    [Required(ErrorMessage = "La fecha de fallecimiento es obligatoria")]
    public DateOnly? FechaFallecimiento { get; set; }

    [StringLength(200, ErrorMessage = "La causa no puede superar 200 caracteres")]
    public string? Causa { get; set; }

    [StringLength(500, ErrorMessage = "Las observaciones no pueden superar 500 caracteres")]
    public string? Observaciones { get; set; }
}

public class FallecimientoRespuestaDto
{
    public DateOnly FechaFallecimiento { get; set; }
    public string? Causa { get; set; }
    public string? Observaciones { get; set; }
}

public class MascotaRespuestaDto
{
    public int Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public int ClienteId { get; set; }
    public string ClienteNombre { get; set; } = string.Empty;
    public int RazaId { get; set; }
    public string RazaNombre { get; set; } = string.Empty;
    public string EspecieNombre { get; set; } = string.Empty;
    public DateOnly? FechaNacimiento { get; set; }
    public string Sexo { get; set; } = string.Empty;
    public decimal? PesoKg { get; set; }
    public string? Color { get; set; }
    public bool Fallecida { get; set; }
    public FallecimientoRespuestaDto? Fallecimiento { get; set; }
}

public class EspecieDto
{
    public int Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
}

public class RazaDto
{
    public int Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public int EspecieId { get; set; }
    public string EspecieNombre { get; set; } = string.Empty;
}
