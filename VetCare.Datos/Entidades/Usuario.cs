namespace VetCare.Datos.Entidades;

public class Usuario : EntidadBase
{
    public string Nombre { get; set; } = string.Empty;
    public string Correo { get; set; } = string.Empty;
    public string HashContraseña { get; set; } = string.Empty;
    public bool Activo { get; set; } = true;

    public int RolId { get; set; }
    public Rol? Rol { get; set; }
}
