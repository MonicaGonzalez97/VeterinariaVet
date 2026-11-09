using Moq;
using VetCare.Datos.Entidades;
using VetCare.Datos.Repositorios;
using VetCare.Negocio.Constantes;
using VetCare.Negocio.Dtos;
using VetCare.Negocio.Excepciones;
using VetCare.Negocio.Servicios;

namespace VetCare.Pruebas.Servicios;

public class ServicioFacturaPruebas
{
    private readonly Mock<IRepositorioFactura> _facturas = new();
    private readonly Mock<IRepositorioCliente> _clientes = new();
    private readonly Mock<IRepositorioProducto> _productos = new();
    private readonly ServicioFactura _servicio;

    public ServicioFacturaPruebas()
    {
        _servicio = new ServicioFactura(_facturas.Object, _clientes.Object, _productos.Object);
    }

    private static Cliente ClienteDueño() => new() { Id = 1, Nombre = "Maria", Apellido = "Perez" };

    private static Producto Shampoo(int stock = 10) => new() { Id = 7, Nombre = "Shampoo antipulgas", Precio = 55m, Stock = stock };

    private void ConfigurarCliente()
    {
        _clientes.Setup(c => c.ObtenerPorIdAsync(1)).ReturnsAsync(ClienteDueño());
        _facturas.Setup(f => f.CrearAsync(It.IsAny<Factura>()))
            .Callback<Factura>(f => f.Id = 3)
            .ReturnsAsync((Factura f) => f);
    }

    private static FacturaSolicitudDto Solicitud(params DetalleFacturaSolicitudDto[] lineas) => new()
    {
        ClienteId = 1,
        Detalles = lineas.ToList()
    };

    [Fact]
    public async Task Emitir_ClienteInexistente_LanzaReglaNegocio()
    {
        _clientes.Setup(c => c.ObtenerPorIdAsync(1)).ReturnsAsync((Cliente?)null);

        await Assert.ThrowsAsync<ExcepcionReglaNegocio>(() =>
            _servicio.EmitirAsync(Solicitud(new DetalleFacturaSolicitudDto { Descripcion = "Consulta", PrecioUnitario = 150m })));
        _facturas.Verify(f => f.CrearAsync(It.IsAny<Factura>()), Times.Never);
    }

    [Fact]
    public async Task Emitir_ProductoSinExistenciasSuficientes_LanzaReglaNegocio()
    {
        ConfigurarCliente();
        _productos.Setup(p => p.ObtenerPorIdAsync(7)).ReturnsAsync(Shampoo(stock: 1));

        await Assert.ThrowsAsync<ExcepcionReglaNegocio>(() =>
            _servicio.EmitirAsync(Solicitud(new DetalleFacturaSolicitudDto { ProductoId = 7, Cantidad = 5 })));
        _facturas.Verify(f => f.CrearAsync(It.IsAny<Factura>()), Times.Never);
    }

    [Fact]
    public async Task Emitir_ProductoMasConceptoLibre_CalculaElTotalYDescuentaExistencias()
    {
        ConfigurarCliente();
        var producto = Shampoo(stock: 10);
        _productos.Setup(p => p.ObtenerPorIdAsync(7)).ReturnsAsync(producto);

        var respuesta = await _servicio.EmitirAsync(Solicitud(
            new DetalleFacturaSolicitudDto { ProductoId = 7, Cantidad = 2 },
            new DetalleFacturaSolicitudDto { Descripcion = " Consulta veterinaria ", Cantidad = 1, PrecioUnitario = 150m }));

        Assert.Equal(260m, respuesta.Total);
        Assert.Equal(8, producto.Stock);
        Assert.Equal(EstadosFactura.Pendiente, respuesta.Estado);
        Assert.Equal(2, respuesta.Detalles.Count);
        Assert.Equal("Consulta veterinaria", respuesta.Detalles[1].Descripcion);
        Assert.Equal(110m, respuesta.Detalles[0].Subtotal);
    }

    [Fact]
    public async Task Emitir_ConceptoLibreSinPrecio_LanzaReglaNegocio()
    {
        ConfigurarCliente();

        await Assert.ThrowsAsync<ExcepcionReglaNegocio>(() =>
            _servicio.EmitirAsync(Solicitud(new DetalleFacturaSolicitudDto { Descripcion = "Consulta", Cantidad = 1 })));
    }

    [Fact]
    public async Task Emitir_ConceptoLibreSinDescripcion_LanzaReglaNegocio()
    {
        ConfigurarCliente();

        await Assert.ThrowsAsync<ExcepcionReglaNegocio>(() =>
            _servicio.EmitirAsync(Solicitud(new DetalleFacturaSolicitudDto { PrecioUnitario = 100m, Cantidad = 1 })));
    }

    [Fact]
    public async Task Pagar_FacturaPendiente_CambiaElEstadoAPagada()
    {
        var factura = new Factura { Id = 3, ClienteId = 1, Estado = EstadosFactura.Pendiente };
        _facturas.Setup(f => f.ObtenerConDetalleAsync(3)).ReturnsAsync(factura);

        var respuesta = await _servicio.PagarAsync(3);

        Assert.Equal(EstadosFactura.Pagada, respuesta.Estado);
        _facturas.Verify(f => f.ActualizarAsync(factura), Times.Once);
    }

    [Fact]
    public async Task Pagar_FacturaAnulada_LanzaReglaNegocio()
    {
        _facturas.Setup(f => f.ObtenerConDetalleAsync(3))
            .ReturnsAsync(new Factura { Id = 3, Estado = EstadosFactura.Anulada });

        await Assert.ThrowsAsync<ExcepcionReglaNegocio>(() => _servicio.PagarAsync(3));
    }

    [Fact]
    public async Task Anular_Factura_DevuelveLosProductosAlInventario()
    {
        var factura = new Factura { Id = 3, ClienteId = 1, Estado = EstadosFactura.Pendiente };
        factura.Detalles.Add(new DetalleFactura { ProductoId = 7, Descripcion = "Shampoo antipulgas", Cantidad = 3, PrecioUnitario = 55m });
        factura.Detalles.Add(new DetalleFactura { Descripcion = "Consulta", Cantidad = 1, PrecioUnitario = 150m });
        _facturas.Setup(f => f.ObtenerConDetalleAsync(3)).ReturnsAsync(factura);
        var producto = Shampoo(stock: 5);
        _productos.Setup(p => p.ObtenerPorIdAsync(7)).ReturnsAsync(producto);

        var respuesta = await _servicio.AnularAsync(3);

        Assert.Equal(8, producto.Stock);
        Assert.Equal(EstadosFactura.Anulada, respuesta.Estado);
    }

    [Fact]
    public async Task Anular_FacturaYaAnulada_LanzaConflicto()
    {
        _facturas.Setup(f => f.ObtenerConDetalleAsync(3))
            .ReturnsAsync(new Factura { Id = 3, Estado = EstadosFactura.Anulada });

        await Assert.ThrowsAsync<ExcepcionConflicto>(() => _servicio.AnularAsync(3));
    }

    [Fact]
    public async Task Obtener_FacturaInexistente_LanzaNoEncontrada()
    {
        _facturas.Setup(f => f.ObtenerConDetalleAsync(99)).ReturnsAsync((Factura?)null);

        await Assert.ThrowsAsync<ExcepcionNoEncontrada>(() => _servicio.ObtenerAsync(99));
    }
}
