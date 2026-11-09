using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VetCare.Api.Modelos;
using VetCare.Negocio.Constantes;
using VetCare.Negocio.Dtos;
using VetCare.Negocio.Servicios;

namespace VetCare.Api.Controladores;

/// <summary>Gestion de mascotas, incluido el registro de fallecimiento (memorial).</summary>
[ApiController]
[Route("api/mascotas")]
[Produces("application/json")]
[Authorize]
public class MascotasControlador : ControllerBase
{
    private const string RolesEscritura = NombresRoles.Administrador + "," + NombresRoles.Recepcionista;

    private readonly IServicioMascota _mascotas;

    public MascotasControlador(IServicioMascota mascotas)
    {
        _mascotas = mascotas;
    }

    /// <summary>Lista las mascotas activas. Con fallecidas=true lista el memorial (mascotas fallecidas).</summary>
    [HttpGet]
    [ProducesResponseType(typeof(List<MascotaRespuestaDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<MascotaRespuestaDto>>> Listar([FromQuery] int? clienteId, [FromQuery] bool fallecidas = false)
    {
        return Ok(await _mascotas.ListarAsync(clienteId, fallecidas));
    }

    /// <summary>Obtiene una mascota (activa o fallecida) por su id.</summary>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(MascotaRespuestaDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(RespuestaError), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<MascotaRespuestaDto>> Obtener(int id)
    {
        return Ok(await _mascotas.ObtenerAsync(id));
    }

    /// <summary>Registra una mascota nueva. Roles: Administrador y Recepcionista.</summary>
    [HttpPost]
    [Authorize(Roles = RolesEscritura)]
    [ProducesResponseType(typeof(MascotaRespuestaDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(RespuestaError), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(RespuestaError), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(RespuestaError), StatusCodes.Status409Conflict)]
    [ProducesResponseType(typeof(RespuestaError), StatusCodes.Status422UnprocessableEntity)]
    public async Task<ActionResult<MascotaRespuestaDto>> Crear([FromBody] MascotaSolicitudDto dto)
    {
        var mascota = await _mascotas.CrearAsync(dto);
        return CreatedAtAction(nameof(Obtener), new { id = mascota.Id }, mascota);
    }

    /// <summary>Actualiza una mascota activa. Roles: Administrador y Recepcionista.</summary>
    [HttpPut("{id:int}")]
    [Authorize(Roles = RolesEscritura)]
    [ProducesResponseType(typeof(MascotaRespuestaDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(RespuestaError), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(RespuestaError), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(RespuestaError), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(RespuestaError), StatusCodes.Status409Conflict)]
    [ProducesResponseType(typeof(RespuestaError), StatusCodes.Status422UnprocessableEntity)]
    public async Task<ActionResult<MascotaRespuestaDto>> Actualizar(int id, [FromBody] MascotaSolicitudDto dto)
    {
        return Ok(await _mascotas.ActualizarAsync(id, dto));
    }

    /// <summary>Elimina una mascota sin historial clinico. Rol: Administrador.</summary>
    [HttpDelete("{id:int}")]
    [Authorize(Roles = NombresRoles.Administrador)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(RespuestaError), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(RespuestaError), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(RespuestaError), StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> Eliminar(int id)
    {
        await _mascotas.EliminarAsync(id);
        return NoContent();
    }

    /// <summary>Registra el fallecimiento de una mascota: sale de la lista activa y pasa al memorial. Roles: Administrador y Veterinario.</summary>
    [HttpPost("{id:int}/fallecimiento")]
    [Authorize(Roles = NombresRoles.Administrador + "," + NombresRoles.Veterinario)]
    [ProducesResponseType(typeof(MascotaRespuestaDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(RespuestaError), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(RespuestaError), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(RespuestaError), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(RespuestaError), StatusCodes.Status409Conflict)]
    [ProducesResponseType(typeof(RespuestaError), StatusCodes.Status422UnprocessableEntity)]
    public async Task<ActionResult<MascotaRespuestaDto>> RegistrarFallecimiento(int id, [FromBody] FallecimientoSolicitudDto dto)
    {
        int? usuarioId = int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var valor) ? valor : null;
        return Ok(await _mascotas.RegistrarFallecimientoAsync(id, dto, usuarioId));
    }

    /// <summary>Revierte un fallecimiento registrado por error. Rol: Administrador.</summary>
    [HttpDelete("{id:int}/fallecimiento")]
    [Authorize(Roles = NombresRoles.Administrador)]
    [ProducesResponseType(typeof(MascotaRespuestaDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(RespuestaError), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(RespuestaError), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(RespuestaError), StatusCodes.Status422UnprocessableEntity)]
    public async Task<ActionResult<MascotaRespuestaDto>> RevertirFallecimiento(int id)
    {
        return Ok(await _mascotas.RevertirFallecimientoAsync(id));
    }
}
