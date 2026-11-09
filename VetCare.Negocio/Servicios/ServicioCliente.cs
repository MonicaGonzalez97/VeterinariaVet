using VetCare.Datos.Entidades;
using VetCare.Datos.Repositorios;
using VetCare.Negocio.Dtos;
using VetCare.Negocio.Excepciones;

namespace VetCare.Negocio.Servicios;

public class ServicioCliente : IServicioCliente
{
    private readonly IRepositorioCliente _clientes;

    public ServicioCliente(IRepositorioCliente clientes)
    {
        _clientes = clientes;
    }

    public async Task<List<ClienteRespuestaDto>> ListarAsync(string? busqueda)
    {
        var clientes = await _clientes.BuscarAsync(busqueda);
        return clientes.Select(Mapear).ToList();
    }

    public async Task<ClienteRespuestaDto> ObtenerAsync(int id)
    {
        var cliente = await _clientes.ObtenerPorIdAsync(id)
            ?? throw new ExcepcionNoEncontrada($"No existe un cliente con id {id}");
        return Mapear(cliente);
    }

    public async Task<ClienteRespuestaDto> CrearAsync(ClienteSolicitudDto dto)
    {
        var documento = dto.Documento.Trim();

        if (await _clientes.BuscarPorDocumentoAsync(documento) != null)
        {
            throw new ExcepcionConflicto("Ya existe un cliente con ese documento");
        }

        var cliente = new Cliente();
        Copiar(dto, cliente);
        await _clientes.CrearAsync(cliente);
        return Mapear(cliente);
    }

    public async Task<ClienteRespuestaDto> ActualizarAsync(int id, ClienteSolicitudDto dto)
    {
        var cliente = await _clientes.ObtenerPorIdAsync(id)
            ?? throw new ExcepcionNoEncontrada($"No existe un cliente con id {id}");

        var otroConMismoDocumento = await _clientes.BuscarPorDocumentoAsync(dto.Documento.Trim());
        if (otroConMismoDocumento != null && otroConMismoDocumento.Id != id)
        {
            throw new ExcepcionConflicto("Otro cliente ya usa ese documento");
        }

        Copiar(dto, cliente);
        await _clientes.ActualizarAsync(cliente);
        return Mapear(cliente);
    }

    public async Task EliminarAsync(int id)
    {
        var cliente = await _clientes.ObtenerPorIdAsync(id)
            ?? throw new ExcepcionNoEncontrada($"No existe un cliente con id {id}");

        if (await _clientes.TieneMascotasAsync(id))
        {
            throw new ExcepcionReglaNegocio("No se puede eliminar un cliente que tiene mascotas registradas");
        }

        await _clientes.EliminarAsync(cliente);
    }

    private static void Copiar(ClienteSolicitudDto dto, Cliente cliente)
    {
        cliente.Nombre = dto.Nombre.Trim();
        cliente.Apellido = dto.Apellido.Trim();
        cliente.Documento = dto.Documento.Trim();
        cliente.Telefono = dto.Telefono.Trim();
        cliente.Correo = string.IsNullOrWhiteSpace(dto.Correo) ? null : dto.Correo.Trim().ToLowerInvariant();
        cliente.Direccion = string.IsNullOrWhiteSpace(dto.Direccion) ? null : dto.Direccion.Trim();
    }

    private static ClienteRespuestaDto Mapear(Cliente cliente)
    {
        return new ClienteRespuestaDto
        {
            Id = cliente.Id,
            Nombre = cliente.Nombre,
            Apellido = cliente.Apellido,
            Documento = cliente.Documento,
            Telefono = cliente.Telefono,
            Correo = cliente.Correo,
            Direccion = cliente.Direccion
        };
    }
}
