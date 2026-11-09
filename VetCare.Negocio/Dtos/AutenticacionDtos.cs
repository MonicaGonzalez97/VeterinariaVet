using System.ComponentModel.DataAnnotations;

namespace VetCare.Negocio.Dtos;

/// <summary>Datos para crear una cuenta nueva. La cuenta se crea con el rol Recepcionista.</summary>
public class RegistroDto
{
    /// <summary>Nombre completo de la persona. Entre 2 y 100 caracteres.</summary>
    /// <example>Ana Lopez</example>
    [Required(ErrorMessage = "El nombre es obligatorio")]
    [StringLength(100, MinimumLength = 2, ErrorMessage = "El nombre debe tener entre 2 y 100 caracteres")]
    public string Nombre { get; set; } = string.Empty;

    /// <summary>Correo electronico. Debe ser unico en el sistema.</summary>
    /// <example>ana@vetcare.com</example>
    [Required(ErrorMessage = "El correo es obligatorio")]
    [EmailAddress(ErrorMessage = "El correo no tiene un formato valido")]
    public string Correo { get; set; } = string.Empty;

    /// <summary>Contraseña de acceso. Minimo 8 caracteres.</summary>
    /// <example>Clave12345*</example>
    [Required(ErrorMessage = "La contraseña es obligatoria")]
    [MinLength(8, ErrorMessage = "La contraseña debe tener al menos 8 caracteres")]
    public string Contraseña { get; set; } = string.Empty;
}

/// <summary>Credenciales para iniciar sesion.</summary>
public class LoginDto
{
    /// <summary>Correo con el que se registro la cuenta.</summary>
    /// <example>admin@vetcare.com</example>
    [Required(ErrorMessage = "El correo es obligatorio")]
    [EmailAddress(ErrorMessage = "El correo no tiene un formato valido")]
    public string Correo { get; set; } = string.Empty;

    /// <summary>Contraseña de la cuenta.</summary>
    /// <example>Admin12345*</example>
    [Required(ErrorMessage = "La contraseña es obligatoria")]
    public string Contraseña { get; set; } = string.Empty;
}

/// <summary>Respuesta de registro e inicio de sesion. El token se envia en la cabecera Authorization: Bearer.</summary>
public class RespuestaAutenticacionDto
{
    /// <summary>Token JWT que se debe enviar en cada peticion protegida.</summary>
    public string Token { get; set; } = string.Empty;

    /// <summary>Fecha y hora (UTC) en que vence el token.</summary>
    public DateTime Expira { get; set; }

    /// <summary>Nombre del usuario autenticado.</summary>
    public string Nombre { get; set; } = string.Empty;

    /// <summary>Correo del usuario autenticado.</summary>
    public string Correo { get; set; } = string.Empty;

    /// <summary>Rol del usuario: Administrador, Veterinario o Recepcionista.</summary>
    public string Rol { get; set; } = string.Empty;
}
