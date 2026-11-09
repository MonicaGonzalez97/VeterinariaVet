using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VetCare.Api.Modelos;
using VetCare.Negocio.Constantes;
using VetCare.Negocio.Dtos;
using VetCare.Negocio.Servicios;

namespace VetCare.Api.Controladores;

/// <summary>Carne de vacunacion: vacunas aplicadas a las mascotas.</summary>
[ApiController]
[Route("api/vacunaciones")]
[Produces("application/json")]
[Authorize]
[ProducesResponseType(typeof(RespuestaError), StatusCodes.Status401Unauthorized)]
public class VacunacionesControlador : ControllerBase
{
    private readonly IServicioVacunacion _vacunacion;

    public VacunacionesControlador(IServicioVacunacion vacunacion)
    {
        _vacunacion = vacunacion;
    }

    /// <summary>Lista las vacunas aplicadas. Con mascotaId muestra el carne de una mascota.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(List<RegistroVacunacionRespuestaDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<RegistroVacunacionRespuestaDto>>> Listar([FromQuery] int? mascotaId)
    {
        return Ok(await _vacunacion.ListarRegistrosAsync(mascotaId));
    }

    /// <summary>Registra la aplicacion de una vacuna. Roles: Administrador y Veterinario.</summary>
    [HttpPost]
    [Authorize(Roles = NombresRoles.Administrador + "," + NombresRoles.Veterinario)]
    [ProducesResponseType(typeof(RegistroVacunacionRespuestaDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(RespuestaError), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(RespuestaError), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(RespuestaError), StatusCodes.Status422UnprocessableEntity)]
    public async Task<ActionResult<RegistroVacunacionRespuestaDto>> Registrar([FromBody] RegistroVacunacionSolicitudDto dto)
    {
        var registro = await _vacunacion.RegistrarAplicacionAsync(dto);
        return StatusCode(StatusCodes.Status201Created, registro);
    }

    /// <summary>Elimina un registro hecho por error. Rol: Administrador.</summary>
    [HttpDelete("{id:int}")]
    [Authorize(Roles = NombresRoles.Administrador)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(RespuestaError), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(RespuestaError), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Eliminar(int id)
    {
        await _vacunacion.EliminarRegistroAsync(id);
        return NoContent();
    }
}
