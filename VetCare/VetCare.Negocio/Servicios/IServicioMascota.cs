using VetCare.Negocio.Dtos;

namespace VetCare.Negocio.Servicios;

public interface IServicioMascota
{
    Task<List<MascotaRespuestaDto>> ListarAsync(int? clienteId, bool fallecidas);
    Task<MascotaRespuestaDto> ObtenerAsync(int id);
    Task<MascotaRespuestaDto> CrearAsync(MascotaSolicitudDto dto);
    Task<MascotaRespuestaDto> ActualizarAsync(int id, MascotaSolicitudDto dto);
    Task EliminarAsync(int id);
    Task<MascotaRespuestaDto> RegistrarFallecimientoAsync(int id, FallecimientoSolicitudDto dto, int? usuarioId);
    Task<MascotaRespuestaDto> RevertirFallecimientoAsync(int id);
    Task<List<EspecieDto>> ListarEspeciesAsync();
    Task<List<RazaDto>> ListarRazasAsync(int? especieId);
}
