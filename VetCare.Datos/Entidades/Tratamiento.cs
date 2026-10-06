namespace VetCare.Datos.Entidades;

public class Tratamiento : EntidadBase
{
    public string Indicaciones { get; set; } = string.Empty;
    public string? Dosis { get; set; }
    public int DuracionDias { get; set; }

    public int ConsultaId { get; set; }
    public Consulta? Consulta { get; set; }

    public int? MedicamentoId { get; set; }
    public Medicamento? Medicamento { get; set; }
}
