using VetCare.Negocio.Dtos;

namespace VetCare.Negocio.Servicios;

public interface IServicioAutenticacion
{
    Task<RespuestaAutenticacionDto> RegistrarAsync(RegistroDto dto);
    Task<RespuestaAutenticacionDto> IniciarSesionAsync(LoginDto dto);
}
