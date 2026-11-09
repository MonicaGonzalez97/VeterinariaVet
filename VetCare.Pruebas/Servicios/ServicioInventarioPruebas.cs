using Moq;
using VetCare.Datos.Entidades;
using VetCare.Datos.Repositorios;
using VetCare.Negocio.Dtos;
using VetCare.Negocio.Excepciones;
using VetCare.Negocio.Servicios;

namespace VetCare.Pruebas.Servicios;

public class ServicioInventarioPruebas
{
    private readonly Mock<IRepositorioMedicamento> _medicamentos = new();
    private readonly Mock<IRepositorioProducto> _productos = new();
    private readonly ServicioInventario _servicio;

    public ServicioInventarioPruebas()
    {
        _servicio = new ServicioInventario(_medicamentos.Object, _productos.Object);
    }

    private static MedicamentoSolicitudDto SolicitudMedicamento() => new()
    {
        Nombre = " Amoxicilina ",
        Presentacion = "Tabletas 250 mg",
        Stock = 40
    };

    private static ProductoSolicitudDto SolicitudProducto() => new()
    {
        Nombre = " Shampoo antipulgas ",
        Categoria = "Higiene",
        Precio = 55m,
        Stock = 25
    };

    // ---------- Medicamentos ----------

    [Fact]
    public async Task CrearMedicamento_NombreDuplicado_LanzaConflicto()
    {
        _medicamentos.Setup(m => m.BuscarPorNombreAsync(It.IsAny<string>()))
            .ReturnsAsync(new Medicamento { Id = 1, Nombre = "Amoxicilina" });

        await Assert.ThrowsAsync<ExcepcionConflicto>(() => _servicio.CrearMedicamentoAsync(SolicitudMedicamento()));
        _medicamentos.Verify(m => m.CrearAsync(It.IsAny<Medicamento>()), Times.Never);
    }

    [Fact]
    public async Task CrearMedicamento_DatosValidos_PersisteConDatosLimpios()
    {
        _medicamentos.Setup(m => m.BuscarPorNombreAsync(It.IsAny<string>())).ReturnsAsync((Medicamento?)null);
        _medicamentos.Setup(m => m.CrearAsync(It.IsAny<Medicamento>()))
            .Callback<Medicamento>(m => m.Id = 4)
            .ReturnsAsync((Medicamento m) => m);

        var respuesta = await _servicio.CrearMedicamentoAsync(SolicitudMedicamento());

        Assert.Equal(4, respuesta.Id);
        Assert.Equal("Amoxicilina", respuesta.Nombre);
        Assert.Equal(40, respuesta.Stock);
    }

    [Fact]
    public async Task ActualizarMedicamento_NombreDeOtroMedicamento_LanzaConflicto()
    {
        _medicamentos.Setup(m => m.ObtenerPorIdAsync(1)).ReturnsAsync(new Medicamento { Id = 1, Nombre = "Meloxicam" });
        _medicamentos.Setup(m => m.BuscarPorNombreAsync(It.IsAny<string>()))
            .ReturnsAsync(new Medicamento { Id = 2, Nombre = "Amoxicilina" });

        await Assert.ThrowsAsync<ExcepcionConflicto>(() => _servicio.ActualizarMedicamentoAsync(1, SolicitudMedicamento()));
    }

    [Fact]
    public async Task EliminarMedicamento_UsadoEnTratamientos_LanzaReglaNegocio()
    {
        _medicamentos.Setup(m => m.ObtenerPorIdAsync(1)).ReturnsAsync(new Medicamento { Id = 1, Nombre = "Amoxicilina" });
        _medicamentos.Setup(m => m.TieneUsoAsync(1)).ReturnsAsync(true);

        await Assert.ThrowsAsync<ExcepcionReglaNegocio>(() => _servicio.EliminarMedicamentoAsync(1));
        _medicamentos.Verify(m => m.EliminarAsync(It.IsAny<Medicamento>()), Times.Never);
    }

    [Fact]
    public async Task ObtenerMedicamento_Inexistente_LanzaNoEncontrada()
    {
        _medicamentos.Setup(m => m.ObtenerPorIdAsync(99)).ReturnsAsync((Medicamento?)null);

        await Assert.ThrowsAsync<ExcepcionNoEncontrada>(() => _servicio.ObtenerMedicamentoAsync(99));
    }

    // ---------- Productos ----------

    [Fact]
    public async Task CrearProducto_NombreDuplicado_LanzaConflicto()
    {
        _productos.Setup(p => p.BuscarPorNombreAsync(It.IsAny<string>()))
            .ReturnsAsync(new Producto { Id = 1, Nombre = "Shampoo antipulgas" });

        await Assert.ThrowsAsync<ExcepcionConflicto>(() => _servicio.CrearProductoAsync(SolicitudProducto()));
        _productos.Verify(p => p.CrearAsync(It.IsAny<Producto>()), Times.Never);
    }

    [Fact]
    public async Task CrearProducto_DatosValidos_PersisteConElPrecioRedondeado()
    {
        _productos.Setup(p => p.BuscarPorNombreAsync(It.IsAny<string>())).ReturnsAsync((Producto?)null);
        _productos.Setup(p => p.CrearAsync(It.IsAny<Producto>()))
            .Callback<Producto>(p => p.Id = 6)
            .ReturnsAsync((Producto p) => p);

        var solicitud = SolicitudProducto();
        solicitud.Precio = 55.456m;

        var respuesta = await _servicio.CrearProductoAsync(solicitud);

        Assert.Equal(6, respuesta.Id);
        Assert.Equal("Shampoo antipulgas", respuesta.Nombre);
        Assert.Equal(55.46m, respuesta.Precio);
    }

    [Fact]
    public async Task ActualizarProducto_DatosValidos_ActualizaPrecioYExistencias()
    {
        var existente = new Producto { Id = 1, Nombre = "Shampoo antipulgas", Precio = 50m, Stock = 5 };
        _productos.Setup(p => p.ObtenerPorIdAsync(1)).ReturnsAsync(existente);
        _productos.Setup(p => p.BuscarPorNombreAsync(It.IsAny<string>())).ReturnsAsync(existente);

        var respuesta = await _servicio.ActualizarProductoAsync(1, SolicitudProducto());

        Assert.Equal(55m, respuesta.Precio);
        Assert.Equal(25, respuesta.Stock);
        _productos.Verify(p => p.ActualizarAsync(existente), Times.Once);
    }

    [Fact]
    public async Task EliminarProducto_QueYaSeFacturo_LanzaReglaNegocio()
    {
        _productos.Setup(p => p.ObtenerPorIdAsync(1)).ReturnsAsync(new Producto { Id = 1, Nombre = "Shampoo antipulgas" });
        _productos.Setup(p => p.TieneUsoAsync(1)).ReturnsAsync(true);

        await Assert.ThrowsAsync<ExcepcionReglaNegocio>(() => _servicio.EliminarProductoAsync(1));
    }

    [Fact]
    public async Task ListarProductos_ConBusqueda_MapeaLosProductosEncontrados()
    {
        _productos.Setup(p => p.BuscarAsync("sham")).ReturnsAsync(new List<Producto>
        {
            new() { Id = 1, Nombre = "Shampoo antipulgas", Precio = 55m, Stock = 25 }
        });

        var lista = await _servicio.ListarProductosAsync("sham");

        Assert.Single(lista);
        Assert.Equal("Shampoo antipulgas", lista[0].Nombre);
    }
}
