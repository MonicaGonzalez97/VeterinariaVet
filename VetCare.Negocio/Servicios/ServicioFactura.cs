using VetCare.Datos.Entidades;
using VetCare.Datos.Repositorios;
using VetCare.Negocio.Constantes;
using VetCare.Negocio.Dtos;
using VetCare.Negocio.Excepciones;

namespace VetCare.Negocio.Servicios;

public interface IServicioFactura
{
    Task<List<FacturaRespuestaDto>> ListarAsync(int? clienteId, string? estado);
    Task<FacturaRespuestaDto> ObtenerAsync(int id);
    Task<FacturaRespuestaDto> EmitirAsync(FacturaSolicitudDto dto);
    Task<FacturaRespuestaDto> PagarAsync(int id);
    Task<FacturaRespuestaDto> AnularAsync(int id);
}

public class ServicioFactura : IServicioFactura
{
    private readonly IRepositorioFactura _facturas;
    private readonly IRepositorioCliente _clientes;
    private readonly IRepositorioProducto _productos;

    public ServicioFactura(IRepositorioFactura facturas, IRepositorioCliente clientes, IRepositorioProducto productos)
    {
        _facturas = facturas;
        _clientes = clientes;
        _productos = productos;
    }

    public async Task<List<FacturaRespuestaDto>> ListarAsync(int? clienteId, string? estado)
    {
        var lista = await _facturas.ListarConDetalleAsync(clienteId, estado);
        return lista.Select(Mapear).ToList();
    }

    public async Task<FacturaRespuestaDto> ObtenerAsync(int id)
    {
        var factura = await _facturas.ObtenerConDetalleAsync(id)
            ?? throw new ExcepcionNoEncontrada($"No existe una factura con id {id}");
        return Mapear(factura);
    }

    public async Task<FacturaRespuestaDto> EmitirAsync(FacturaSolicitudDto dto)
    {
        var cliente = await _clientes.ObtenerPorIdAsync(dto.ClienteId)
            ?? throw new ExcepcionReglaNegocio("El cliente indicado no existe");

        var factura = new Factura
        {
            ClienteId = cliente.Id,
            Cliente = cliente,
            Fecha = DateTime.UtcNow,
            Estado = EstadosFactura.Pendiente
        };

        decimal total = 0m;

        foreach (var linea in dto.Detalles)
        {
            var detalle = new DetalleFactura { Cantidad = linea.Cantidad };

            if (linea.ProductoId.HasValue)
            {
                var producto = await _productos.ObtenerPorIdAsync(linea.ProductoId.Value)
                    ?? throw new ExcepcionReglaNegocio("El producto indicado no existe");

                if (producto.Stock < linea.Cantidad)
                {
                    throw new ExcepcionReglaNegocio(
                        $"Existencias insuficientes de {producto.Nombre} (disponibles: {producto.Stock})");
                }

                producto.Stock -= linea.Cantidad;
                detalle.ProductoId = producto.Id;
                detalle.Producto = producto;
                detalle.Descripcion = producto.Nombre;
                detalle.PrecioUnitario = producto.Precio;
            }
            else
            {
                if (string.IsNullOrWhiteSpace(linea.Descripcion))
                {
                    throw new ExcepcionReglaNegocio("Cada linea sin producto necesita una descripcion");
                }

                if (!linea.PrecioUnitario.HasValue || linea.PrecioUnitario.Value <= 0)
                {
                    throw new ExcepcionReglaNegocio("Cada linea sin producto necesita un precio mayor que cero");
                }

                detalle.Descripcion = linea.Descripcion.Trim();
                detalle.PrecioUnitario = Math.Round(linea.PrecioUnitario.Value, 2);
            }

            total += detalle.Cantidad * detalle.PrecioUnitario;
            factura.Detalles.Add(detalle);
        }

        factura.Total = Math.Round(total, 2);

        // Un DbContext por peticion: la factura y el descuento de existencias se guardan juntos
        await _facturas.CrearAsync(factura);

        var guardada = await _facturas.ObtenerConDetalleAsync(factura.Id) ?? factura;
        return Mapear(guardada);
    }

    public async Task<FacturaRespuestaDto> PagarAsync(int id)
    {
        var factura = await _facturas.ObtenerConDetalleAsync(id)
            ?? throw new ExcepcionNoEncontrada($"No existe una factura con id {id}");

        if (factura.Estado == EstadosFactura.Pagada)
        {
            throw new ExcepcionConflicto("La factura ya estaba pagada");
        }

        if (factura.Estado == EstadosFactura.Anulada)
        {
            throw new ExcepcionReglaNegocio("No se puede pagar una factura anulada");
        }

        factura.Estado = EstadosFactura.Pagada;
        await _facturas.ActualizarAsync(factura);
        return Mapear(factura);
    }

    public async Task<FacturaRespuestaDto> AnularAsync(int id)
    {
        var factura = await _facturas.ObtenerConDetalleAsync(id)
            ?? throw new ExcepcionNoEncontrada($"No existe una factura con id {id}");

        if (factura.Estado == EstadosFactura.Anulada)
        {
            throw new ExcepcionConflicto("La factura ya estaba anulada");
        }

        // Se devuelven al inventario las unidades vendidas
        foreach (var detalle in factura.Detalles)
        {
            if (!detalle.ProductoId.HasValue)
            {
                continue;
            }

            var producto = await _productos.ObtenerPorIdAsync(detalle.ProductoId.Value);
            if (producto != null)
            {
                producto.Stock += detalle.Cantidad;
            }
        }

        factura.Estado = EstadosFactura.Anulada;
        await _facturas.ActualizarAsync(factura);
        return Mapear(factura);
    }

    private static FacturaRespuestaDto Mapear(Factura factura)
    {
        return new FacturaRespuestaDto
        {
            Id = factura.Id,
            Fecha = factura.Fecha,
            ClienteId = factura.ClienteId,
            ClienteNombre = factura.Cliente == null ? string.Empty : $"{factura.Cliente.Nombre} {factura.Cliente.Apellido}",
            Total = factura.Total,
            Estado = factura.Estado,
            Detalles = factura.Detalles
                .Select(d => new DetalleFacturaRespuestaDto
                {
                    Id = d.Id,
                    ProductoId = d.ProductoId,
                    Descripcion = d.Descripcion,
                    Cantidad = d.Cantidad,
                    PrecioUnitario = d.PrecioUnitario,
                    Subtotal = Math.Round(d.Cantidad * d.PrecioUnitario, 2)
                })
                .ToList()
        };
    }
}
