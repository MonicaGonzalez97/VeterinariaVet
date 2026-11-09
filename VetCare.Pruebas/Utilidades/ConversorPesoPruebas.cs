using VetCare.Negocio.Excepciones;
using VetCare.Negocio.Utilidades;

namespace VetCare.Pruebas.Utilidades;

public class ConversorPesoPruebas
{
    [Fact]
    public void ALibras_DiezKilogramos_Devuelve2205()
    {
        Assert.Equal(22.05m, ConversorPeso.ALibras(10m));
    }

    [Fact]
    public void AKilogramos_CienLibras_Devuelve4536()
    {
        Assert.Equal(45.36m, ConversorPeso.AKilogramos(100m));
    }

    [Fact]
    public void ObtenerKilogramos_SinPeso_DevuelveNull()
    {
        Assert.Null(ConversorPeso.ObtenerKilogramos(null, "lb"));
    }

    [Fact]
    public void ObtenerKilogramos_UnidadVacia_TomaKilogramosPorDefecto()
    {
        Assert.Equal(8.50m, ConversorPeso.ObtenerKilogramos(8.5m, null));
        Assert.Equal(8.50m, ConversorPeso.ObtenerKilogramos(8.5m, "  "));
    }

    [Fact]
    public void ObtenerKilogramos_UnidadEnMayusculas_ConvierteLibras()
    {
        Assert.Equal(12.50m, ConversorPeso.ObtenerKilogramos(27.56m, "LB"));
    }

    [Fact]
    public void ObtenerKilogramos_LimiteMaximoEnLibras_EsValidoYUnPocoMasFalla()
    {
        Assert.Equal(200.00m, ConversorPeso.ObtenerKilogramos(440.92m, "lb"));
        Assert.Throws<ExcepcionReglaNegocio>(() => ConversorPeso.ObtenerKilogramos(441m, "lb"));
    }
}
