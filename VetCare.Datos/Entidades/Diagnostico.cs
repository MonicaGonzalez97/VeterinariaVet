namespace VetCare.Datos.Entidades;

public class Diagnostico : EntidadBase
{
    public string Descripcion { get; set; } = string.Empty;
    public string? Gravedad { get; set; }

    public int ConsultaId { get; set; }
    public Consulta? Consulta { get; set; }
}
