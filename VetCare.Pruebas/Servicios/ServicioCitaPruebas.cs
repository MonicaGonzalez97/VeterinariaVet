using Moq;
using VetCare.Datos.Entidades;
using VetCare.Datos.Repositorios;
using VetCare.Negocio.Constantes;
using VetCare.Negocio.Dtos;
using VetCare.Negocio.Excepciones;
using VetCare.Negocio.Servicios;

namespace VetCare.Pruebas.Servicios;

public class ServicioCitaPruebas
{
    private readonly Mock<IRepositorioCita> _citas = new();
    private readonly Mock<IRepositorioMascota> _mascotas = new();
    private readonly Mock<IRepositorioVeterinario> _veterinarios = new();
    private readonly ServicioCita _servicio;

    public ServicioCitaPruebas()
    {
        _servicio = new ServicioCita(_citas.Object, _mascotas.Object, _veterinarios.Object);
    }

    private static CitaSolicitudDto Solicitud(DateTime? fechaHora = null) => new()
    {
        MascotaId = 1,
        VeterinarioId = 2,
        FechaHora = fechaHora ?? DateTime.UtcNow.AddDays(3),
        Motivo = "  Vacunacion anual "
    };

    private static Mascota MascotaActiva() => new() { Id = 1, Nombre = "Firulais" };
    private static Veterinario UnVeterinario() => new() { Id = 2, Nombre = "Laura", Apellido = "Ramirez" };

    private static Cita CitaConEstado(string estado) => new()
    {
        Id = 5,
        MascotaId = 1,
        Mascota = MascotaActiva(),
        VeterinarioId = 2,
        Veterinario = UnVeterinario(),
        FechaHora = DateTime.UtcNow.AddDays(2),
        Motivo = "Control",
        Estado = estado
    };

    private void ConfigurarEntidadesValidas()
    {
        _mascotas.Setup(m => m.ObtenerPorIdAsync(1)).ReturnsAsync(MascotaActiva());
        _veterinarios.Setup(v => v.ObtenerPorIdAsync(2)).ReturnsAsync(UnVeterinario());
    }

    [Fact]
    public async Task Crear_FechaPasada_LanzaReglaNegocio()
    {
        ConfigurarEntidadesValidas();

        await Assert.ThrowsAsync<ExcepcionReglaNegocio>(() =>
            _servicio.CrearAsync(Solicitud(DateTime.UtcNow.AddDays(-5))));
        _citas.Verify(c => c.CrearAsync(It.IsAny<Cita>()), Times.Never);
    }

    [Fact]
    public async Task Crear_MascotaFallecida_LanzaReglaNegocio()
    {
        ConfigurarEntidadesValidas();
        var fallecida = MascotaActiva();
        fallecida.Fallecida = true;
        _mascotas.Setup(m => m.ObtenerPorIdAsync(1)).ReturnsAsync(fallecida);

        await Assert.ThrowsAsync<ExcepcionReglaNegocio>(() => _servicio.CrearAsync(Solicitud()));
    }

    [Fact]
    public async Task Crear_VeterinarioInexistente_LanzaReglaNegocio()
    {
        ConfigurarEntidadesValidas();
        _veterinarios.Setup(v => v.ObtenerPorIdAsync(2)).ReturnsAsync((Veterinario?)null);

        await Assert.ThrowsAsync<ExcepcionReglaNegocio>(() => _servicio.CrearAsync(Solicitud()));
    }

    [Fact]
    public async Task Crear_VeterinarioConCitaCercana_LanzaConflicto()
    {
        ConfigurarEntidadesValidas();
        _citas.Setup(c => c.ExisteCruceAsync(2, It.IsAny<DateTime>(), null)).ReturnsAsync(true);

        await Assert.ThrowsAsync<ExcepcionConflicto>(() => _servicio.CrearAsync(Solicitud()));
        _citas.Verify(c => c.CrearAsync(It.IsAny<Cita>()), Times.Never);
    }

    [Fact]
    public async Task Crear_DatosValidos_GuardaLaCitaComoProgramadaEnUtc()
    {
        ConfigurarEntidadesValidas();
        Cita? creada = null;
        _citas.Setup(c => c.CrearAsync(It.IsAny<Cita>()))
            .Callback<Cita>(c => { c.Id = 9; creada = c; })
            .ReturnsAsync((Cita c) => c);

        var respuesta = await _servicio.CrearAsync(Solicitud());

        Assert.NotNull(creada);
        Assert.Equal(EstadosCita.Programada, creada!.Estado);
        Assert.Equal(DateTimeKind.Utc, creada.FechaHora.Kind);
        Assert.Equal("Vacunacion anual", creada.Motivo);
        Assert.Equal(9, respuesta.Id);
        Assert.Equal("Laura Ramirez", respuesta.VeterinarioNombre);
    }

    [Fact]
    public async Task Obtener_CitaInexistente_LanzaNoEncontrada()
    {
        _citas.Setup(c => c.ObtenerConDetalleAsync(99)).ReturnsAsync((Cita?)null);

        await Assert.ThrowsAsync<ExcepcionNoEncontrada>(() => _servicio.ObtenerAsync(99));
    }

    [Fact]
    public async Task Actualizar_CitaYaCompletada_LanzaReglaNegocio()
    {
        _citas.Setup(c => c.ObtenerConDetalleAsync(5)).ReturnsAsync(CitaConEstado(EstadosCita.Completada));

        await Assert.ThrowsAsync<ExcepcionReglaNegocio>(() => _servicio.ActualizarAsync(5, Solicitud()));
        _citas.Verify(c => c.ActualizarAsync(It.IsAny<Cita>()), Times.Never);
    }

    [Fact]
    public async Task Cancelar_CitaProgramada_CambiaElEstadoACancelada()
    {
        var cita = CitaConEstado(EstadosCita.Programada);
        _citas.Setup(c => c.ObtenerConDetalleAsync(5)).ReturnsAsync(cita);

        var respuesta = await _servicio.CancelarAsync(5);

        Assert.Equal(EstadosCita.Cancelada, respuesta.Estado);
        _citas.Verify(c => c.ActualizarAsync(cita), Times.Once);
    }

    [Fact]
    public async Task Cancelar_CitaCompletada_LanzaReglaNegocio()
    {
        _citas.Setup(c => c.ObtenerConDetalleAsync(5)).ReturnsAsync(CitaConEstado(EstadosCita.Completada));

        await Assert.ThrowsAsync<ExcepcionReglaNegocio>(() => _servicio.CancelarAsync(5));
    }

    [Fact]
    public async Task Cancelar_CitaYaCancelada_LanzaConflicto()
    {
        _citas.Setup(c => c.ObtenerConDetalleAsync(5)).ReturnsAsync(CitaConEstado(EstadosCita.Cancelada));

        await Assert.ThrowsAsync<ExcepcionConflicto>(() => _servicio.CancelarAsync(5));
    }
}
