using VetCare.Negocio.Dtos;

namespace VetCare.Negocio.Servicios;

public interface IServicioCliente
{
    Task<List<ClienteRespuestaDto>> ListarAsync(string? busqueda);
    Task<ClienteRespuestaDto> ObtenerAsync(int id);
    Task<ClienteRespuestaDto> CrearAsync(ClienteSolicitudDto dto);
    Task<ClienteRespuestaDto> ActualizarAsync(int id, ClienteSolicitudDto dto);
    Task EliminarAsync(int id);
}
