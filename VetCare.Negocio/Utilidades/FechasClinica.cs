namespace VetCare.Negocio.Utilidades;

/// <summary>Reglas de fechas comunes. La clinica trabaja en hora de Guatemala (UTC-6, sin horario de verano).</summary>
public static class FechasClinica
{
    /// <summary>Fecha maxima permitida para hechos ya ocurridos (hoy, con un dia de margen por zona horaria).</summary>
    public static DateOnly LimiteFechaHoy()
    {
        return DateOnly.FromDateTime(DateTime.UtcNow).AddDays(1);
    }

    /// <summary>Inicio del dia de hoy en la clinica.</summary>
    public static DateTime InicioDeHoy()
    {
        return DateTime.UtcNow.AddHours(-6).Date;
    }

    /// <summary>PostgreSQL (timestamptz) solo acepta fechas con Kind=Utc.</summary>
    public static DateTime ComoUtc(DateTime fecha)
    {
        return DateTime.SpecifyKind(fecha, DateTimeKind.Utc);
    }
}
