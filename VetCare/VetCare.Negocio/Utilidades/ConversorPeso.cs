using System.Globalization;
using VetCare.Negocio.Excepciones;

namespace VetCare.Negocio.Utilidades;

/// <summary>Convierte el peso de las mascotas entre kilogramos y libras. 
/// En la base de datos siempre se guarda en kilogramos.</summary>
public static class ConversorPeso
{
    public const decimal LibrasPorKilogramo = 2.2046226218m;
    public const decimal PesoMinimoKg = 0.01m;
    public const decimal PesoMaximoKg = 200m;
    public const string Kilogramos = "kg";
    public const string Libras = "lb";

    public static decimal ALibras(decimal kilogramos)
    {
        return Math.Round(kilogramos * LibrasPorKilogramo, 2, MidpointRounding.AwayFromZero);
    }

    public static decimal AKilogramos(decimal libras)
    {
        return Math.Round(libras / LibrasPorKilogramo, 2, MidpointRounding.AwayFromZero);
    }

    /// <summary>
    /// Devuelve el peso en kilogramos a partir del valor y la unidad elegidos por el usuario.
    /// Si no hay peso devuelve null. Si la unidad no es kg ni lb, o el peso queda fuera del rango permitido, lanza una regla de negocio (422).
    /// </summary>
    public static decimal? ObtenerKilogramos(decimal? peso, string? unidad)
    {
        if (!peso.HasValue)
        {
            return null;
        }

        var unidadNormalizada = string.IsNullOrWhiteSpace(unidad) ? Kilogramos : unidad.Trim().ToLowerInvariant();

        decimal kilogramos = unidadNormalizada switch
        {
            Kilogramos => Math.Round(peso.Value, 2, MidpointRounding.AwayFromZero),
            Libras => AKilogramos(peso.Value),
            _ => throw new ExcepcionReglaNegocio("La unidad de peso debe ser kg o lb")
        };

        if (kilogramos < PesoMinimoKg || kilogramos > PesoMaximoKg)
        {
            throw new ExcepcionReglaNegocio(string.Format(
                CultureInfo.InvariantCulture,
                "El peso debe estar entre {0} kg y {1} kg ({2} lb y {3} lb)",
                PesoMinimoKg, PesoMaximoKg, ALibras(PesoMinimoKg), ALibras(PesoMaximoKg)));
        }

        return kilogramos;
    }
}
