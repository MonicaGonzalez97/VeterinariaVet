using System.ComponentModel.DataAnnotations;

namespace VetCare.Negocio.Dtos;

/// <summary>Datos para crear o actualizar un cliente (dueño de mascotas).</summary>
public class ClienteSolicitudDto
{
    /// <summary>Nombres del cliente. Entre 2 y 80 caracteres.</summary>
    /// <example>Maria</example>
    [Required(ErrorMessage = "El nombre es obligatorio")]
    [StringLength(80, MinimumLength = 2, ErrorMessage = "El nombre debe tener entre 2 y 80 caracteres")]
    public string Nombre { get; set; } = string.Empty;

    /// <summary>Apellidos del cliente. Entre 2 y 80 caracteres.</summary>
    /// <example>Perez</example>
    [Required(ErrorMessage = "El apellido es obligatorio")]
    [StringLength(80, MinimumLength = 2, ErrorMessage = "El apellido debe tener entre 2 y 80 caracteres")]
    public string Apellido { get; set; } = string.Empty;

    /// <summary>Documento personal de identificacion (DPI): 13 digitos. Debe ser unico.</summary>
    /// <example>1234567890123</example>
    [Required(ErrorMessage = "El documento es obligatorio")]
    [RegularExpression(@"^\d{13}$", ErrorMessage = "El documento (DPI) debe tener 13 digitos")]
    public string Documento { get; set; } = string.Empty;

    /// <summary>Telefono de contacto: 8 digitos.</summary>
    /// <example>55512345</example>
    [Required(ErrorMessage = "El telefono es obligatorio")]
    [RegularExpression(@"^\d{8}$", ErrorMessage = "El telefono debe tener 8 digitos")]
    public string Telefono { get; set; } = string.Empty;

    /// <summary>Correo electronico (opcional).</summary>
    /// <example>maria@correo.com</example>
    [EmailAddress(ErrorMessage = "El correo no tiene un formato valido")]
    public string? Correo { get; set; }

    /// <summary>Direccion de residencia (opcional). Maximo 200 caracteres.</summary>
    /// <example>Zona 1, Quetzaltenango</example>
    [StringLength(200, ErrorMessage = "La direccion no puede superar 200 caracteres")]
    public string? Direccion { get; set; }
}

/// <summary>Cliente registrado en la clinica.</summary>
public class ClienteRespuestaDto
{
    /// <summary>Identificador del cliente.</summary>
    public int Id { get; set; }

    /// <summary>Nombres del cliente.</summary>
    public string Nombre { get; set; } = string.Empty;

    /// <summary>Apellidos del cliente.</summary>
    public string Apellido { get; set; } = string.Empty;

    /// <summary>Documento personal de identificacion (DPI).</summary>
    public string Documento { get; set; } = string.Empty;

    /// <summary>Telefono de contacto.</summary>
    public string Telefono { get; set; } = string.Empty;

    /// <summary>Correo electronico, si lo tiene.</summary>
    public string? Correo { get; set; }

    /// <summary>Direccion, si la tiene.</summary>
    public string? Direccion { get; set; }
}
