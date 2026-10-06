using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VetCare.Api.Modelos;
using VetCare.Negocio.Dtos;
using VetCare.Negocio.Servicios;

namespace VetCare.Api.Controladores;

/// <summary>Registro e inicio de sesion. Son los unicos endpoints publicos de la API.</summary>
[ApiController]
[Route("api/autenticacion")]
[Produces("application/json")]
[AllowAnonymous]
public class AutenticacionControlador : ControllerBase
{
    private readonly IServicioAutenticacion _autenticacion;

    public AutenticacionControlador(IServicioAutenticacion autenticacion)
    {
        _autenticacion = autenticacion;
    }

    /// <summary>Registra un usuario nuevo con el rol Recepcionista y devuelve su token JWT.</summary>
    [HttpPost("registro")]
    [ProducesResponseType(typeof(RespuestaAutenticacionDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(RespuestaError), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(RespuestaError), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<RespuestaAutenticacionDto>> Registrar([FromBody] RegistroDto dto)
    {
        var respuesta = await _autenticacion.RegistrarAsync(dto);
        return StatusCode(StatusCodes.Status201Created, respuesta);
    }

    /// <summary>Inicia sesion con correo y contraseña y devuelve el token JWT.</summary>
    [HttpPost("login")]
    [ProducesResponseType(typeof(RespuestaAutenticacionDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(RespuestaError), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(RespuestaError), StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<RespuestaAutenticacionDto>> IniciarSesion([FromBody] LoginDto dto)
    {
        var respuesta = await _autenticacion.IniciarSesionAsync(dto);
        return Ok(respuesta);
    }
}
