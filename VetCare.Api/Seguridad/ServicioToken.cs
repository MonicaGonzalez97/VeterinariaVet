using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using VetCare.Datos.Entidades;
using VetCare.Negocio.Dtos;
using VetCare.Negocio.Servicios;

namespace VetCare.Api.Seguridad;

public class ServicioToken : IServicioToken
{
    private readonly OpcionesJwt _opciones;

    public ServicioToken(IOptions<OpcionesJwt> opciones)
    {
        _opciones = opciones.Value;
    }

    public RespuestaAutenticacionDto GenerarToken(Usuario usuario)
    {
        var nombreRol = usuario.Rol?.Nombre ?? string.Empty;
        var expira = DateTime.UtcNow.AddMinutes(_opciones.MinutosExpiracion);

        var claves = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_opciones.Clave));
        var credenciales = new SigningCredentials(claves, SecurityAlgorithms.HmacSha256);

        var descriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.NameIdentifier, usuario.Id.ToString()),
                new Claim(ClaimTypes.Name, usuario.Nombre),
                new Claim(ClaimTypes.Email, usuario.Correo),
                new Claim(ClaimTypes.Role, nombreRol)
            }),
            Expires = expira,
            Issuer = _opciones.Emisor,
            Audience = _opciones.Audiencia,
            SigningCredentials = credenciales
        };

        var token = new JsonWebTokenHandler().CreateToken(descriptor);

        return new RespuestaAutenticacionDto
        {
            Token = token,
            Expira = expira,
            Nombre = usuario.Nombre,
            Correo = usuario.Correo,
            Rol = nombreRol
        };
    }
}
