using Moq;
using VetCare.Datos.Entidades;
using VetCare.Datos.Repositorios;
using VetCare.Negocio.Dtos;
using VetCare.Negocio.Excepciones;
using VetCare.Negocio.Servicios;

namespace VetCare.Pruebas.Servicios;

public class ServicioClientePruebas
{
    private readonly Mock<IRepositorioCliente> _clientes = new();
    private readonly ServicioCliente _servicio;

    public ServicioClientePruebas()
    {
        _servicio = new ServicioCliente(_clientes.Object);
    }

    private static ClienteSolicitudDto SolicitudValida() => new()
    {
        Nombre = "  Maria ",
        Apellido = "Perez",
        Documento = "1234567890123",
        Telefono = "55512345",
        Correo = "MARIA@correo.com",
        Direccion = "Zona 1, Quetzaltenango"
    };

    private static Cliente ClienteExistente(int id = 1) => new()
    {
        Id = id,
        Nombre = "Maria",
        Apellido = "Perez",
        Documento = "1234567890123",
        Telefono = "55512345"
    };

    [Fact]
    public async Task Crear_DocumentoDuplicado_LanzaConflicto()
    {
        _clientes.Setup(c => c.BuscarPorDocumentoAsync("1234567890123")).ReturnsAsync(ClienteExistente());

        await Assert.ThrowsAsync<ExcepcionConflicto>(() => _servicio.CrearAsync(SolicitudValida()));
        _clientes.Verify(c => c.CrearAsync(It.IsAny<Cliente>()), Times.Never);
    }

    [Fact]
    public async Task Crear_DatosValidos_PersisteClienteConDatosLimpios()
    {
        _clientes.Setup(c => c.BuscarPorDocumentoAsync(It.IsAny<string>())).ReturnsAsync((Cliente?)null);

        Cliente? creado = null;
        _clientes.Setup(c => c.CrearAsync(It.IsAny<Cliente>()))
            .Callback<Cliente>(c => { c.Id = 10; creado = c; })
            .ReturnsAsync((Cliente c) => c);

        var respuesta = await _servicio.CrearAsync(SolicitudValida());

        Assert.NotNull(creado);
        Assert.Equal("Maria", creado!.Nombre);
        Assert.Equal("maria@correo.com", creado.Correo);
        Assert.Equal(10, respuesta.Id);
        Assert.Equal("Perez", respuesta.Apellido);
    }

    [Fact]
    public async Task Obtener_ClienteInexistente_LanzaNoEncontrada()
    {
        _clientes.Setup(c => c.ObtenerPorIdAsync(99)).ReturnsAsync((Cliente?)null);

        await Assert.ThrowsAsync<ExcepcionNoEncontrada>(() => _servicio.ObtenerAsync(99));
    }

    [Fact]
    public async Task Actualizar_DocumentoDeOtroCliente_LanzaConflicto()
    {
        _clientes.Setup(c => c.ObtenerPorIdAsync(1)).ReturnsAsync(ClienteExistente(1));
        _clientes.Setup(c => c.BuscarPorDocumentoAsync("1234567890123")).ReturnsAsync(ClienteExistente(2));

        await Assert.ThrowsAsync<ExcepcionConflicto>(() => _servicio.ActualizarAsync(1, SolicitudValida()));
        _clientes.Verify(c => c.ActualizarAsync(It.IsAny<Cliente>()), Times.Never);
    }

    [Fact]
    public async Task Actualizar_ConSuMismoDocumento_ActualizaLosDatos()
    {
        var existente = ClienteExistente(1);
        _clientes.Setup(c => c.ObtenerPorIdAsync(1)).ReturnsAsync(existente);
        _clientes.Setup(c => c.BuscarPorDocumentoAsync("1234567890123")).ReturnsAsync(existente);

        var solicitud = SolicitudValida();
        solicitud.Telefono = "44411122";

        var respuesta = await _servicio.ActualizarAsync(1, solicitud);

        Assert.Equal("44411122", respuesta.Telefono);
        _clientes.Verify(c => c.ActualizarAsync(existente), Times.Once);
    }

    [Fact]
    public async Task Eliminar_ClienteConMascotas_LanzaReglaNegocio()
    {
        var cliente = ClienteExistente();
        _clientes.Setup(c => c.ObtenerPorIdAsync(1)).ReturnsAsync(cliente);
        _clientes.Setup(c => c.TieneMascotasAsync(1)).ReturnsAsync(true);

        await Assert.ThrowsAsync<ExcepcionReglaNegocio>(() => _servicio.EliminarAsync(1));
        _clientes.Verify(c => c.EliminarAsync(It.IsAny<Cliente>()), Times.Never);
    }

    [Fact]
    public async Task Eliminar_ClienteSinMascotas_EliminaElRegistro()
    {
        var cliente = ClienteExistente();
        _clientes.Setup(c => c.ObtenerPorIdAsync(1)).ReturnsAsync(cliente);
        _clientes.Setup(c => c.TieneMascotasAsync(1)).ReturnsAsync(false);

        await _servicio.EliminarAsync(1);

        _clientes.Verify(c => c.EliminarAsync(cliente), Times.Once);
    }

    [Fact]
    public async Task Listar_ConBusqueda_MapeaLosClientesADtos()
    {
        _clientes.Setup(c => c.BuscarAsync("mar"))
            .ReturnsAsync(new List<Cliente> { ClienteExistente(1), ClienteExistente(2) });

        var lista = await _servicio.ListarAsync("mar");

        Assert.Equal(2, lista.Count);
        Assert.All(lista, c => Assert.Equal("Maria", c.Nombre));
    }
}
