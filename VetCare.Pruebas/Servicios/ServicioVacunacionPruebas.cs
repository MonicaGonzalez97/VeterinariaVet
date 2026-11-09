using Moq;
using VetCare.Datos.Entidades;
using VetCare.Datos.Repositorios;
using VetCare.Negocio.Dtos;
using VetCare.Negocio.Excepciones;
using VetCare.Negocio.Servicios;

namespace VetCare.Pruebas.Servicios;

public class ServicioVacunacionPruebas
{
    private readonly Mock<IRepositorioVacuna> _vacunas = new();
    private readonly Mock<IRepositorioRegistroVacunacion> _registros = new();
    private readonly Mock<IRepositorioMascota> _mascotas = new();
    private readonly Mock<IRepositorioVeterinario> _veterinarios = new();
    private readonly ServicioVacunacion _servicio;

    public ServicioVacunacionPruebas()
    {
        _servicio = new ServicioVacunacion(_vacunas.Object, _registros.Object, _mascotas.Object, _veterinarios.Object);
    }

    private static DateOnly Hoy() => DateOnly.FromDateTime(DateTime.UtcNow);

    private static Mascota MascotaActiva() => new() { Id = 1, Nombre = "Firulais" };
    private static Vacuna VacunaRabia() => new() { Id = 2, Nombre = "Rabia", IntervaloDias = 365 };
    private static Veterinario UnVeterinario() => new() { Id = 3, Nombre = "Laura", Apellido = "Ramirez" };

    private void ConfigurarEntidadesValidas()
    {
        _mascotas.Setup(m => m.ObtenerPorIdAsync(1)).ReturnsAsync(MascotaActiva());
        _vacunas.Setup(v => v.ObtenerPorIdAsync(2)).ReturnsAsync(VacunaRabia());
        _veterinarios.Setup(v => v.ObtenerPorIdAsync(3)).ReturnsAsync(UnVeterinario());
    }

    private static RegistroVacunacionSolicitudDto Solicitud(DateOnly fecha) => new()
    {
        MascotaId = 1,
        VacunaId = 2,
        VeterinarioId = 3,
        FechaAplicacion = fecha,
        Lote = " L-2026 "
    };

    // ---------- Catalogo ----------

    [Fact]
    public async Task CrearVacuna_NombreDuplicado_LanzaConflicto()
    {
        _vacunas.Setup(v => v.BuscarPorNombreAsync("Rabia")).ReturnsAsync(VacunaRabia());

        await Assert.ThrowsAsync<ExcepcionConflicto>(() =>
            _servicio.CrearVacunaAsync(new VacunaSolicitudDto { Nombre = "Rabia", IntervaloDias = 365 }));
        _vacunas.Verify(v => v.CrearAsync(It.IsAny<Vacuna>()), Times.Never);
    }

    [Fact]
    public async Task CrearVacuna_DatosValidos_PersisteLaVacuna()
    {
        _vacunas.Setup(v => v.BuscarPorNombreAsync(It.IsAny<string>())).ReturnsAsync((Vacuna?)null);
        _vacunas.Setup(v => v.CrearAsync(It.IsAny<Vacuna>()))
            .Callback<Vacuna>(v => v.Id = 8)
            .ReturnsAsync((Vacuna v) => v);

        var respuesta = await _servicio.CrearVacunaAsync(new VacunaSolicitudDto { Nombre = " Parvovirus ", IntervaloDias = 365 });

        Assert.Equal(8, respuesta.Id);
        Assert.Equal("Parvovirus", respuesta.Nombre);
        Assert.Equal(365, respuesta.IntervaloDias);
    }

    [Fact]
    public async Task EliminarVacuna_ConAplicacionesRegistradas_LanzaReglaNegocio()
    {
        _vacunas.Setup(v => v.ObtenerPorIdAsync(2)).ReturnsAsync(VacunaRabia());
        _vacunas.Setup(v => v.TieneRegistrosAsync(2)).ReturnsAsync(true);

        await Assert.ThrowsAsync<ExcepcionReglaNegocio>(() => _servicio.EliminarVacunaAsync(2));
        _vacunas.Verify(v => v.EliminarAsync(It.IsAny<Vacuna>()), Times.Never);
    }

    [Fact]
    public async Task EliminarVacuna_SinAplicaciones_EliminaElRegistro()
    {
        var vacuna = VacunaRabia();
        _vacunas.Setup(v => v.ObtenerPorIdAsync(2)).ReturnsAsync(vacuna);
        _vacunas.Setup(v => v.TieneRegistrosAsync(2)).ReturnsAsync(false);

        await _servicio.EliminarVacunaAsync(2);

        _vacunas.Verify(v => v.EliminarAsync(vacuna), Times.Once);
    }

    // ---------- Carne de vacunacion ----------

    [Fact]
    public async Task Registrar_MascotaFallecida_LanzaReglaNegocio()
    {
        ConfigurarEntidadesValidas();
        var fallecida = MascotaActiva();
        fallecida.Fallecida = true;
        _mascotas.Setup(m => m.ObtenerPorIdAsync(1)).ReturnsAsync(fallecida);

        await Assert.ThrowsAsync<ExcepcionReglaNegocio>(() => _servicio.RegistrarAplicacionAsync(Solicitud(Hoy())));
        _registros.Verify(r => r.CrearAsync(It.IsAny<RegistroVacunacion>()), Times.Never);
    }

    [Fact]
    public async Task Registrar_VacunaInexistente_LanzaReglaNegocio()
    {
        ConfigurarEntidadesValidas();
        _vacunas.Setup(v => v.ObtenerPorIdAsync(2)).ReturnsAsync((Vacuna?)null);

        await Assert.ThrowsAsync<ExcepcionReglaNegocio>(() => _servicio.RegistrarAplicacionAsync(Solicitud(Hoy())));
    }

    [Fact]
    public async Task Registrar_FechaFutura_LanzaReglaNegocio()
    {
        ConfigurarEntidadesValidas();

        await Assert.ThrowsAsync<ExcepcionReglaNegocio>(() =>
            _servicio.RegistrarAplicacionAsync(Solicitud(Hoy().AddDays(20))));
    }

    [Fact]
    public async Task Registrar_ProximaDosisAnteriorALaAplicacion_LanzaReglaNegocio()
    {
        ConfigurarEntidadesValidas();
        var fecha = Hoy().AddDays(-5);
        var solicitud = Solicitud(fecha);
        solicitud.ProximaDosis = fecha.AddDays(-1);

        await Assert.ThrowsAsync<ExcepcionReglaNegocio>(() => _servicio.RegistrarAplicacionAsync(solicitud));
    }

    [Fact]
    public async Task Registrar_SinProximaDosis_LaCalculaConElIntervaloDeLaVacuna()
    {
        ConfigurarEntidadesValidas();
        RegistroVacunacion? creado = null;
        _registros.Setup(r => r.CrearAsync(It.IsAny<RegistroVacunacion>()))
            .Callback<RegistroVacunacion>(r => { r.Id = 11; creado = r; })
            .ReturnsAsync((RegistroVacunacion r) => r);

        var fecha = Hoy().AddDays(-10);
        var respuesta = await _servicio.RegistrarAplicacionAsync(Solicitud(fecha));

        Assert.NotNull(creado);
        Assert.Equal(fecha.AddDays(365), creado!.ProximaDosis);
        Assert.Equal("L-2026", creado.Lote);
        Assert.Equal(11, respuesta.Id);
        Assert.Equal("Rabia", respuesta.VacunaNombre);
        Assert.Equal("Firulais", respuesta.MascotaNombre);
    }

    [Fact]
    public async Task ListarRegistros_MapeaLosNombresDeMascotaVacunaYVeterinario()
    {
        var registro = new RegistroVacunacion
        {
            Id = 1,
            MascotaId = 1,
            Mascota = MascotaActiva(),
            VacunaId = 2,
            Vacuna = VacunaRabia(),
            VeterinarioId = 3,
            Veterinario = UnVeterinario(),
            FechaAplicacion = Hoy()
        };
        _registros.Setup(r => r.ListarPorMascotaAsync(1)).ReturnsAsync(new List<RegistroVacunacion> { registro });

        var lista = await _servicio.ListarRegistrosAsync(1);

        Assert.Single(lista);
        Assert.Equal("Laura Ramirez", lista[0].VeterinarioNombre);
    }
}
