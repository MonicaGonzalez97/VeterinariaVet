using Moq;
using VetCare.Datos.Entidades;
using VetCare.Datos.Repositorios;
using VetCare.Negocio.Dtos;
using VetCare.Negocio.Excepciones;
using VetCare.Negocio.Servicios;

namespace VetCare.Pruebas.Servicios;

public class ServicioMascotaPruebas
{
    private readonly Mock<IRepositorioMascota> _mascotas = new();
    private readonly Mock<IRepositorioCliente> _clientes = new();
    private readonly Mock<IRepositorioRaza> _razas = new();
    private readonly Mock<IRepositorio<Especie>> _especies = new();
    private readonly ServicioMascota _servicio;

    public ServicioMascotaPruebas()
    {
        _servicio = new ServicioMascota(_mascotas.Object, _clientes.Object, _razas.Object, _especies.Object);
    }

    private static MascotaSolicitudDto SolicitudValida() => new()
    {
        Nombre = "Firulais",
        ClienteId = 1,
        RazaId = 2,
        Sexo = "Macho",
        FechaNacimiento = new DateOnly(2020, 5, 10),
        PesoKg = 12.5m
    };

    private static Cliente ClienteDueño() => new() { Id = 1, Nombre = "Maria", Apellido = "Perez" };

    private static Raza RazaLabrador() => new()
    {
        Id = 2,
        Nombre = "Labrador Retriever",
        EspecieId = 1,
        Especie = new Especie { Id = 1, Nombre = "Perro" }
    };

    private static Mascota MascotaActiva(int id = 7) => new()
    {
        Id = id,
        Nombre = "Firulais",
        ClienteId = 1,
        Cliente = ClienteDueño(),
        RazaId = 2,
        Raza = RazaLabrador(),
        Sexo = "Macho",
        FechaNacimiento = new DateOnly(2018, 1, 1),
        Fallecida = false
    };

    private void ConfigurarClienteYRazaValidos()
    {
        _clientes.Setup(c => c.ObtenerPorIdAsync(1)).ReturnsAsync(ClienteDueño());
        _razas.Setup(r => r.ObtenerPorIdAsync(2)).ReturnsAsync(RazaLabrador());
    }

    // ---------- Crear ----------

    [Fact]
    public async Task Crear_ClienteInexistente_LanzaReglaNegocio()
    {
        _clientes.Setup(c => c.ObtenerPorIdAsync(1)).ReturnsAsync((Cliente?)null);

        await Assert.ThrowsAsync<ExcepcionReglaNegocio>(() => _servicio.CrearAsync(SolicitudValida()));
        _mascotas.Verify(m => m.CrearAsync(It.IsAny<Mascota>()), Times.Never);
    }

    [Fact]
    public async Task Crear_RazaInexistente_LanzaReglaNegocio()
    {
        _clientes.Setup(c => c.ObtenerPorIdAsync(1)).ReturnsAsync(ClienteDueño());
        _razas.Setup(r => r.ObtenerPorIdAsync(2)).ReturnsAsync((Raza?)null);

        await Assert.ThrowsAsync<ExcepcionReglaNegocio>(() => _servicio.CrearAsync(SolicitudValida()));
    }

    [Fact]
    public async Task Crear_FechaNacimientoFutura_LanzaReglaNegocio()
    {
        ConfigurarClienteYRazaValidos();
        var solicitud = SolicitudValida();
        solicitud.FechaNacimiento = DateOnly.FromDateTime(DateTime.UtcNow).AddDays(30);

        await Assert.ThrowsAsync<ExcepcionReglaNegocio>(() => _servicio.CrearAsync(solicitud));
        _mascotas.Verify(m => m.CrearAsync(It.IsAny<Mascota>()), Times.Never);
    }

    [Fact]
    public async Task Crear_NombreRepetidoParaElMismoCliente_LanzaConflicto()
    {
        ConfigurarClienteYRazaValidos();
        _mascotas.Setup(m => m.ExisteNombreParaClienteAsync(1, "Firulais", null)).ReturnsAsync(true);

        await Assert.ThrowsAsync<ExcepcionConflicto>(() => _servicio.CrearAsync(SolicitudValida()));
    }

    [Fact]
    public async Task Crear_DatosValidos_PersisteLaMascotaYDevuelveSuDetalle()
    {
        ConfigurarClienteYRazaValidos();
        _mascotas.Setup(m => m.ExisteNombreParaClienteAsync(1, "Firulais", null)).ReturnsAsync(false);

        Mascota? creada = null;
        _mascotas.Setup(m => m.CrearAsync(It.IsAny<Mascota>()))
            .Callback<Mascota>(m => { m.Id = 7; creada = m; })
            .ReturnsAsync((Mascota m) => m);
        _mascotas.Setup(m => m.ObtenerConDetalleAsync(7)).ReturnsAsync(MascotaActiva(7));

        var respuesta = await _servicio.CrearAsync(SolicitudValida());

        Assert.NotNull(creada);
        Assert.Equal("Macho", creada!.Sexo);
        Assert.Equal(12.5m, creada.PesoKg);
        Assert.Equal(7, respuesta.Id);
        Assert.Equal("Perro", respuesta.EspecieNombre);
        Assert.Equal("Maria Perez", respuesta.ClienteNombre);
        Assert.False(respuesta.Fallecida);
    }

    // ---------- Consultar, actualizar y eliminar ----------

    [Fact]
    public async Task Obtener_MascotaInexistente_LanzaNoEncontrada()
    {
        _mascotas.Setup(m => m.ObtenerConDetalleAsync(99)).ReturnsAsync((Mascota?)null);

        await Assert.ThrowsAsync<ExcepcionNoEncontrada>(() => _servicio.ObtenerAsync(99));
    }

    [Fact]
    public async Task Actualizar_MascotaFallecida_LanzaReglaNegocio()
    {
        var fallecida = MascotaActiva();
        fallecida.Fallecida = true;
        _mascotas.Setup(m => m.ObtenerConDetalleAsync(7)).ReturnsAsync(fallecida);

        await Assert.ThrowsAsync<ExcepcionReglaNegocio>(() => _servicio.ActualizarAsync(7, SolicitudValida()));
        _mascotas.Verify(m => m.ActualizarAsync(It.IsAny<Mascota>()), Times.Never);
    }

    [Fact]
    public async Task Eliminar_MascotaConHistorialClinico_LanzaReglaNegocio()
    {
        _mascotas.Setup(m => m.ObtenerConDetalleAsync(7)).ReturnsAsync(MascotaActiva());
        _mascotas.Setup(m => m.TieneHistorialAsync(7)).ReturnsAsync(true);

        await Assert.ThrowsAsync<ExcepcionReglaNegocio>(() => _servicio.EliminarAsync(7));
        _mascotas.Verify(m => m.EliminarAsync(It.IsAny<Mascota>()), Times.Never);
    }

    [Fact]
    public async Task Eliminar_MascotaSinHistorial_EliminaElRegistro()
    {
        var mascota = MascotaActiva();
        _mascotas.Setup(m => m.ObtenerConDetalleAsync(7)).ReturnsAsync(mascota);
        _mascotas.Setup(m => m.TieneHistorialAsync(7)).ReturnsAsync(false);

        await _servicio.EliminarAsync(7);

        _mascotas.Verify(m => m.EliminarAsync(mascota), Times.Once);
    }

    // ---------- Fallecimiento / memorial ----------

    [Fact]
    public async Task RegistrarFallecimiento_DatosValidos_MarcaLaMascotaYGuardaElRegistro()
    {
        var mascota = MascotaActiva();
        _mascotas.Setup(m => m.ObtenerConDetalleAsync(7)).ReturnsAsync(mascota);
        _mascotas.Setup(m => m.RegistrarFallecimientoAsync(mascota, It.IsAny<RegistroFallecimiento>()))
            .Callback<Mascota, RegistroFallecimiento>((m, r) => { m.Fallecida = true; m.RegistroFallecimiento = r; })
            .Returns(Task.CompletedTask);

        var fecha = DateOnly.FromDateTime(DateTime.UtcNow);
        var respuesta = await _servicio.RegistrarFallecimientoAsync(
            7,
            new FallecimientoSolicitudDto { FechaFallecimiento = fecha, Causa = " Vejez ", Observaciones = null },
            usuarioId: 5);

        Assert.True(respuesta.Fallecida);
        Assert.NotNull(respuesta.Fallecimiento);
        Assert.Equal(fecha, respuesta.Fallecimiento!.FechaFallecimiento);
        Assert.Equal("Vejez", respuesta.Fallecimiento.Causa);
        _mascotas.Verify(m => m.RegistrarFallecimientoAsync(
            mascota,
            It.Is<RegistroFallecimiento>(r => r.UsuarioRegistroId == 5 && r.FechaFallecimiento == fecha)),
            Times.Once);
    }

    [Fact]
    public async Task RegistrarFallecimiento_MascotaYaFallecida_LanzaConflicto()
    {
        var fallecida = MascotaActiva();
        fallecida.Fallecida = true;
        _mascotas.Setup(m => m.ObtenerConDetalleAsync(7)).ReturnsAsync(fallecida);

        await Assert.ThrowsAsync<ExcepcionConflicto>(() => _servicio.RegistrarFallecimientoAsync(
            7, new FallecimientoSolicitudDto { FechaFallecimiento = new DateOnly(2024, 1, 1) }, null));
    }

    [Fact]
    public async Task RegistrarFallecimiento_FechaFutura_LanzaReglaNegocio()
    {
        _mascotas.Setup(m => m.ObtenerConDetalleAsync(7)).ReturnsAsync(MascotaActiva());
        var futura = DateOnly.FromDateTime(DateTime.UtcNow).AddDays(10);

        await Assert.ThrowsAsync<ExcepcionReglaNegocio>(() => _servicio.RegistrarFallecimientoAsync(
            7, new FallecimientoSolicitudDto { FechaFallecimiento = futura }, null));
    }

    [Fact]
    public async Task RegistrarFallecimiento_AnteriorAlNacimiento_LanzaReglaNegocio()
    {
        _mascotas.Setup(m => m.ObtenerConDetalleAsync(7)).ReturnsAsync(MascotaActiva()); // nace en 2018

        await Assert.ThrowsAsync<ExcepcionReglaNegocio>(() => _servicio.RegistrarFallecimientoAsync(
            7, new FallecimientoSolicitudDto { FechaFallecimiento = new DateOnly(2015, 1, 1) }, null));
        _mascotas.Verify(m => m.RegistrarFallecimientoAsync(It.IsAny<Mascota>(), It.IsAny<RegistroFallecimiento>()), Times.Never);
    }

    [Fact]
    public async Task RevertirFallecimiento_MascotaActiva_LanzaReglaNegocio()
    {
        _mascotas.Setup(m => m.ObtenerConDetalleAsync(7)).ReturnsAsync(MascotaActiva());

        await Assert.ThrowsAsync<ExcepcionReglaNegocio>(() => _servicio.RevertirFallecimientoAsync(7));
    }

    [Fact]
    public async Task ListarFallecidas_PideAlRepositorioSoloLasFallecidasYMapeaElRegistro()
    {
        var fallecida = MascotaActiva();
        fallecida.Fallecida = true;
        fallecida.RegistroFallecimiento = new RegistroFallecimiento
        {
            FechaFallecimiento = new DateOnly(2025, 3, 3),
            Causa = "Vejez"
        };
        _mascotas.Setup(m => m.ListarConDetalleAsync(null, true)).ReturnsAsync(new List<Mascota> { fallecida });

        var lista = await _servicio.ListarAsync(null, fallecidas: true);

        Assert.Single(lista);
        Assert.True(lista[0].Fallecida);
        Assert.Equal(new DateOnly(2025, 3, 3), lista[0].Fallecimiento!.FechaFallecimiento);
    }

    // ---------- Catalogos ----------

    [Fact]
    public async Task ListarRazas_IncluyeElNombreDeLaEspecie()
    {
        _razas.Setup(r => r.ListarPorEspecieAsync(1)).ReturnsAsync(new List<Raza> { RazaLabrador() });

        var lista = await _servicio.ListarRazasAsync(1);

        Assert.Single(lista);
        Assert.Equal("Labrador Retriever", lista[0].Nombre);
        Assert.Equal("Perro", lista[0].EspecieNombre);
    }

    [Fact]
    public async Task ListarEspecies_DevuelveLasEspeciesOrdenadasPorNombre()
    {
        _especies.Setup(e => e.ListarAsync()).ReturnsAsync(new List<Especie>
        {
            new() { Id = 2, Nombre = "Gato" },
            new() { Id = 1, Nombre = "Ave" }
        });

        var lista = await _servicio.ListarEspeciesAsync();

        Assert.Equal(new[] { "Ave", "Gato" }, lista.Select(e => e.Nombre).ToArray());
    }
}
