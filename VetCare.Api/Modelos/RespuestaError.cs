namespace VetCare.Api.Modelos;

/// <summary>Formato unico de error para todos los codigos 4xx y 5xx de la API.</summary>
public class RespuestaError
{
    public int Estado { get; set; }
    public string Mensaje { get; set; } = string.Empty;
    public Dictionary<string, string[]>? Detalles { get; set; }
}
