using VetCare.Datos.Entidades;
using VetCare.Datos.Repositorios;
using VetCare.Negocio.Dtos;
using VetCare.Negocio.Excepciones;

namespace VetCare.Negocio.Servicios;

public interface IServicioInventario
{
    Task<List<MedicamentoRespuestaDto>> ListarMedicamentosAsync(string? busqueda);
    Task<MedicamentoRespuestaDto> ObtenerMedicamentoAsync(int id);
    Task<MedicamentoRespuestaDto> CrearMedicamentoAsync(MedicamentoSolicitudDto dto);
    Task<MedicamentoRespuestaDto> ActualizarMedicamentoAsync(int id, MedicamentoSolicitudDto dto);
    Task EliminarMedicamentoAsync(int id);

    Task<List<ProductoRespuestaDto>> ListarProductosAsync(string? busqueda);
    Task<ProductoRespuestaDto> ObtenerProductoAsync(int id);
    Task<ProductoRespuestaDto> CrearProductoAsync(ProductoSolicitudDto dto);
    Task<ProductoRespuestaDto> ActualizarProductoAsync(int id, ProductoSolicitudDto dto);
    Task EliminarProductoAsync(int id);
}

public class ServicioInventario : IServicioInventario
{
    private readonly IRepositorioMedicamento _medicamentos;
    private readonly IRepositorioProducto _productos;

    public ServicioInventario(IRepositorioMedicamento medicamentos, IRepositorioProducto productos)
    {
        _medicamentos = medicamentos;
        _productos = productos;
    }

    // ---------- Medicamentos ----------

    public async Task<List<MedicamentoRespuestaDto>> ListarMedicamentosAsync(string? busqueda)
    {
        var lista = await _medicamentos.BuscarAsync(busqueda);
        return lista.Select(MapearMedicamento).ToList();
    }

    public async Task<MedicamentoRespuestaDto> ObtenerMedicamentoAsync(int id)
    {
        var medicamento = await _medicamentos.ObtenerPorIdAsync(id)
            ?? throw new ExcepcionNoEncontrada($"No existe un medicamento con id {id}");
        return MapearMedicamento(medicamento);
    }

    public async Task<MedicamentoRespuestaDto> CrearMedicamentoAsync(MedicamentoSolicitudDto dto)
    {
        if (await _medicamentos.BuscarPorNombreAsync(dto.Nombre) != null)
        {
            throw new ExcepcionConflicto("Ya existe un medicamento con ese nombre");
        }

        var medicamento = new Medicamento();
        CopiarMedicamento(dto, medicamento);
        await _medicamentos.CrearAsync(medicamento);
        return MapearMedicamento(medicamento);
    }

    public async Task<MedicamentoRespuestaDto> ActualizarMedicamentoAsync(int id, MedicamentoSolicitudDto dto)
    {
        var medicamento = await _medicamentos.ObtenerPorIdAsync(id)
            ?? throw new ExcepcionNoEncontrada($"No existe un medicamento con id {id}");

        var otro = await _medicamentos.BuscarPorNombreAsync(dto.Nombre);
        if (otro != null && otro.Id != id)
        {
            throw new ExcepcionConflicto("Ya existe otro medicamento con ese nombre");
        }

        CopiarMedicamento(dto, medicamento);
        await _medicamentos.ActualizarAsync(medicamento);
        return MapearMedicamento(medicamento);
    }

    public async Task EliminarMedicamentoAsync(int id)
    {
        var medicamento = await _medicamentos.ObtenerPorIdAsync(id)
            ?? throw new ExcepcionNoEncontrada($"No existe un medicamento con id {id}");

        if (await _medicamentos.TieneUsoAsync(id))
        {
            throw new ExcepcionReglaNegocio("No se puede eliminar un medicamento que ya se uso en tratamientos");
        }

        await _medicamentos.EliminarAsync(medicamento);
    }

    // ---------- Productos ----------

    public async Task<List<ProductoRespuestaDto>> ListarProductosAsync(string? busqueda)
    {
        var lista = await _productos.BuscarAsync(busqueda);
        return lista.Select(MapearProducto).ToList();
    }

    public async Task<ProductoRespuestaDto> ObtenerProductoAsync(int id)
    {
        var producto = await _productos.ObtenerPorIdAsync(id)
            ?? throw new ExcepcionNoEncontrada($"No existe un producto con id {id}");
        return MapearProducto(producto);
    }

    public async Task<ProductoRespuestaDto> CrearProductoAsync(ProductoSolicitudDto dto)
    {
        if (await _productos.BuscarPorNombreAsync(dto.Nombre) != null)
        {
            throw new ExcepcionConflicto("Ya existe un producto con ese nombre");
        }

        var producto = new Producto();
        CopiarProducto(dto, producto);
        await _productos.CrearAsync(producto);
        return MapearProducto(producto);
    }

    public async Task<ProductoRespuestaDto> ActualizarProductoAsync(int id, ProductoSolicitudDto dto)
    {
        var producto = await _productos.ObtenerPorIdAsync(id)
            ?? throw new ExcepcionNoEncontrada($"No existe un producto con id {id}");

        var otro = await _productos.BuscarPorNombreAsync(dto.Nombre);
        if (otro != null && otro.Id != id)
        {
            throw new ExcepcionConflicto("Ya existe otro producto con ese nombre");
        }

        CopiarProducto(dto, producto);
        await _productos.ActualizarAsync(producto);
        return MapearProducto(producto);
    }

    public async Task EliminarProductoAsync(int id)
    {
        var producto = await _productos.ObtenerPorIdAsync(id)
            ?? throw new ExcepcionNoEncontrada($"No existe un producto con id {id}");

        if (await _productos.TieneUsoAsync(id))
        {
            throw new ExcepcionReglaNegocio("No se puede eliminar un producto que ya aparece en facturas");
        }

        await _productos.EliminarAsync(producto);
    }

    private static void CopiarMedicamento(MedicamentoSolicitudDto dto, Medicamento medicamento)
    {
        medicamento.Nombre = dto.Nombre.Trim();
        medicamento.Presentacion = string.IsNullOrWhiteSpace(dto.Presentacion) ? null : dto.Presentacion.Trim();
        medicamento.Stock = dto.Stock;
    }

    private static void CopiarProducto(ProductoSolicitudDto dto, Producto producto)
    {
        producto.Nombre = dto.Nombre.Trim();
        producto.Categoria = string.IsNullOrWhiteSpace(dto.Categoria) ? null : dto.Categoria.Trim();
        producto.Precio = Math.Round(dto.Precio, 2);
        producto.Stock = dto.Stock;
    }

    private static MedicamentoRespuestaDto MapearMedicamento(Medicamento medicamento)
    {
        return new MedicamentoRespuestaDto
        {
            Id = medicamento.Id,
            Nombre = medicamento.Nombre,
            Presentacion = medicamento.Presentacion,
            Stock = medicamento.Stock
        };
    }

    private static ProductoRespuestaDto MapearProducto(Producto producto)
    {
        return new ProductoRespuestaDto
        {
            Id = producto.Id,
            Nombre = producto.Nombre,
            Categoria = producto.Categoria,
            Precio = producto.Precio,
            Stock = producto.Stock
        };
    }
}
