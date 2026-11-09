using Moq;
using VetCare.Datos.Entidades;
using VetCare.Datos.Repositorios;
using VetCare.Negocio.Dtos;
using VetCare.Negocio.Excepciones;
using VetCare.Negocio.Servicios;

namespace VetCare.Pruebas.Servicios;

public class ServicioVeterinarioPruebas
{
    private readonly Mock<IRepositorioVeterinario> _veterinarios = new();
    private readonly ServicioVeterinario _servicio;

    public ServicioVeterinarioPruebas()
    {
        _servicio = new ServicioVeterinario(_veterinarios.Object);
    }

    private static VeterinarioSolicitudDto Solicitud() => new()
    {
        Nombre = "  Laura ",
        Apellido = "Ramirez",
        NumeroColegiado = "VET-1024",
        Especialidad = "Medicina interna"
    };

    private static Veterinario Existente(int id = 1) => new()
    {
        Id = id,
        Nombre = "Laura",
        Apellido = "Ramirez",
        NumeroColegiado = "VET-1024"
    };

    [Fact]
    public async Task Crear_ColegiadoDuplicado_LanzaConflicto()
    {
        _veterinarios.Setup(v => v.BuscarPorColegiadoAsync("VET-1024")).ReturnsAsync(Existente());

        await Assert.ThrowsAsync<ExcepcionConflicto>(() => _servicio.CrearAsync(Solicitud()));
        _veterinarios.Verify(v => v.CrearAsync(It.IsAny<Veterinario>()), Times.Never);
    }

    [Fact]
    public async Task Crear_DatosValidos_PersisteConDatosLimpios()
    {
        _veterinarios.Setup(v => v.BuscarPorColegiadoAsync(It.IsAny<string>())).ReturnsAsync((Veterinario?)null);

        Veterinario? creado = null;
        _veterinarios.Setup(v => v.CrearAsync(It.IsAny<Veterinario>()))
            .Callback<Veterinario>(v => { v.Id = 5; creado = v; })
            .ReturnsAsync((Veterinario v) => v);

        var respuesta = await _servicio.CrearAsync(Solicitud());

        Assert.NotNull(creado);
        Assert.Equal("Laura", creado!.Nombre);
        Assert.Equal(5, respuesta.Id);
        Assert.Equal("Laura Ramirez", respuesta.NombreCompleto);
    }

    [Fact]
    public async Task Obtener_VeterinarioInexistente_LanzaNoEncontrada()
    {
        _veterinarios.Setup(v => v.ObtenerPorIdAsync(99)).ReturnsAsync((Veterinario?)null);

        await Assert.ThrowsAsync<ExcepcionNoEncontrada>(() => _servicio.ObtenerAsync(99));
    }

    [Fact]
    public async Task Actualizar_ColegiadoDeOtroVeterinario_LanzaConflicto()
    {
        _veterinarios.Setup(v => v.ObtenerPorIdAsync(1)).ReturnsAsync(Existente(1));
        _veterinarios.Setup(v => v.BuscarPorColegiadoAsync("VET-1024")).ReturnsAsync(Existente(2));

        await Assert.ThrowsAsync<ExcepcionConflicto>(() => _servicio.ActualizarAsync(1, Solicitud()));
        _veterinarios.Verify(v => v.ActualizarAsync(It.IsAny<Veterinario>()), Times.Never);
    }

    [Fact]
    public async Task Actualizar_DatosValidos_ActualizaLaEspecialidad()
    {
        var existente = Existente(1);
        _veterinarios.Setup(v => v.ObtenerPorIdAsync(1)).ReturnsAsync(existente);
        _veterinarios.Setup(v => v.BuscarPorColegiadoAsync("VET-1024")).ReturnsAsync(existente);

        var solicitud = Solicitud();
        solicitud.Especialidad = "Cirugia";

        var respuesta = await _servicio.ActualizarAsync(1, solicitud);

        Assert.Equal("Cirugia", respuesta.Especialidad);
        _veterinarios.Verify(v => v.ActualizarAsync(existente), Times.Once);
    }

    [Fact]
    public async Task Eliminar_VeterinarioConActividad_LanzaReglaNegocio()
    {
        _veterinarios.Setup(v => v.ObtenerPorIdAsync(1)).ReturnsAsync(Existente());
        _veterinarios.Setup(v => v.TieneActividadAsync(1)).ReturnsAsync(true);

        await Assert.ThrowsAsync<ExcepcionReglaNegocio>(() => _servicio.EliminarAsync(1));
        _veterinarios.Verify(v => v.EliminarAsync(It.IsAny<Veterinario>()), Times.Never);
    }

    [Fact]
    public async Task Eliminar_VeterinarioSinActividad_EliminaElRegistro()
    {
        var existente = Existente();
        _veterinarios.Setup(v => v.ObtenerPorIdAsync(1)).ReturnsAsync(existente);
        _veterinarios.Setup(v => v.TieneActividadAsync(1)).ReturnsAsync(false);

        await _servicio.EliminarAsync(1);

        _veterinarios.Verify(v => v.EliminarAsync(existente), Times.Once);
    }

    [Fact]
    public async Task Listar_MapeaElNombreCompletoDeCadaVeterinario()
    {
        _veterinarios.Setup(v => v.ListarAsync()).ReturnsAsync(new List<Veterinario> { Existente(1), Existente(2) });

        var lista = await _servicio.ListarAsync();

        Assert.Equal(2, lista.Count);
        Assert.All(lista, v => Assert.Equal("Laura Ramirez", v.NombreCompleto));
    }
}
