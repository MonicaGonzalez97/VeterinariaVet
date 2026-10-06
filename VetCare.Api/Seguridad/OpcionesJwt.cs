namespace VetCare.Api.Seguridad;

public class OpcionesJwt
{
    public const string Seccion = "Jwt";

    public string Clave { get; set; } = string.Empty;
    public string Emisor { get; set; } = "VetCare.Api";
    public string Audiencia { get; set; } = "VetCare.Clientes";
    public int MinutosExpiracion { get; set; } = 120;
}
