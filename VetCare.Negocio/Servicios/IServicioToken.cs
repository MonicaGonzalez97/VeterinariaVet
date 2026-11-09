using VetCare.Datos.Entidades;
using VetCare.Negocio.Dtos;

namespace VetCare.Negocio.Servicios;

/// <summary>Genera el JWT. La implementacion vive en la capa de presentacion (infraestructura de seguridad).</summary>
public interface IServicioToken
{
    RespuestaAutenticacionDto GenerarToken(Usuario usuario);
}
