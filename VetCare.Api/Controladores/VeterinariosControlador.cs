using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VetCare.Api.Modelos;
using VetCare.Negocio.Constantes;
using VetCare.Negocio.Dtos;
using VetCare.Negocio.Servicios;

namespace VetCare.Api.Controladores;

/// <summary>Gestion de los veterinarios de la clinica.</summary>
[ApiController]
[Route("api/veterinarios")]
[Produces("application/json")]
[Authorize]
[ProducesResponseType(typeof(RespuestaError), StatusCodes.Status401Unauthorized)]
public class VeterinariosControlador : ControllerBase
{
    private readonly IServicioVeterinario _veterinarios;

    public VeterinariosControlador(IServicioVeterinario veterinarios)
    {
        _veterinarios = veterinarios;
    }

    /// <summary>Lista los veterinarios ordenados por apellido.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(List<VeterinarioRespuestaDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<VeterinarioRespuestaDto>>> Listar()
    {
        return Ok(await _veterinarios.ListarAsync());
    }

    /// <summary>Obtiene un veterinario por su id.</summary>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(VeterinarioRespuestaDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(RespuestaError), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<VeterinarioRespuestaDto>> Obtener(int id)
    {
        return Ok(await _veterinarios.ObtenerAsync(id));
    }

    /// <summary>Crea un veterinario. Rol: Administrador.</summary>
    [HttpPost]
    [Authorize(Roles = NombresRoles.Administrador)]
    [ProducesResponseType(typeof(VeterinarioRespuestaDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(RespuestaError), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(RespuestaError), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(RespuestaError), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<VeterinarioRespuestaDto>> Crear([FromBody] VeterinarioSolicitudDto dto)
    {
        var veterinario = await _veterinarios.CrearAsync(dto);
        return CreatedAtAction(nameof(Obtener), new { id = veterinario.Id }, veterinario);
    }

    /// <summary>Actualiza un veterinario. Rol: Administrador.</summary>
    [HttpPut("{id:int}")]
    [Authorize(Roles = NombresRoles.Administrador)]
    [ProducesResponseType(typeof(VeterinarioRespuestaDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(RespuestaError), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(RespuestaError), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(RespuestaError), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(RespuestaError), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<VeterinarioRespuestaDto>> Actualizar(int id, [FromBody] VeterinarioSolicitudDto dto)
    {
        return Ok(await _veterinarios.ActualizarAsync(id, dto));
    }

    /// <summary>Elimina un veterinario sin actividad. Rol: Administrador.</summary>
    [HttpDelete("{id:int}")]
    [Authorize(Roles = NombresRoles.Administrador)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(RespuestaError), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(RespuestaError), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(RespuestaError), StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> Eliminar(int id)
    {
        await _veterinarios.EliminarAsync(id);
        return NoContent();
    }
}
