using System.ComponentModel.DataAnnotations;

namespace VetCare.Negocio.Dtos;

public class ClienteSolicitudDto
{
    [Required(ErrorMessage = "El nombre es obligatorio")]
    [StringLength(80, MinimumLength = 2, ErrorMessage = "El nombre debe tener entre 2 y 80 caracteres")]
    public string Nombre { get; set; } = string.Empty;

    [Required(ErrorMessage = "El apellido es obligatorio")]
    [StringLength(80, MinimumLength = 2, ErrorMessage = "El apellido debe tener entre 2 y 80 caracteres")]
    public string Apellido { get; set; } = string.Empty;

    [Required(ErrorMessage = "El documento es obligatorio")]
    [RegularExpression(@"^\d{13}$", ErrorMessage = "El documento (DPI) debe tener 13 digitos")]
    public string Documento { get; set; } = string.Empty;

    [Required(ErrorMessage = "El telefono es obligatorio")]
    [RegularExpression(@"^\d{8}$", ErrorMessage = "El telefono debe tener 8 digitos")]
    public string Telefono { get; set; } = string.Empty;

    [EmailAddress(ErrorMessage = "El correo no tiene un formato valido")]
    public string? Correo { get; set; }

    [StringLength(200, ErrorMessage = "La direccion no puede superar 200 caracteres")]
    public string? Direccion { get; set; }
}

public class ClienteRespuestaDto
{
    public int Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string Apellido { get; set; } = string.Empty;
    public string Documento { get; set; } = string.Empty;
    public string Telefono { get; set; } = string.Empty;
    public string? Correo { get; set; }
    public string? Direccion { get; set; }
}
