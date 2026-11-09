using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VetCare.Api.Modelos;
using VetCare.Negocio.Constantes;
using VetCare.Negocio.Dtos;
using VetCare.Negocio.Servicios;

namespace VetCare.Api.Controladores;

/// <summary>Catalogo de vacunas disponibles en la clinica.</summary>
[ApiController]
[Route("api/vacunas")]
[Produces("application/json")]
[Authorize]
[ProducesResponseType(typeof(RespuestaError), StatusCodes.Status401Unauthorized)]
public class VacunasControlador : ControllerBase
{
    private const string RolesEscritura = NombresRoles.Administrador + "," + NombresRoles.Veterinario;

    private readonly IServicioVacunacion _vacunacion;

    public VacunasControlador(IServicioVacunacion vacunacion)
    {
        _vacunacion = vacunacion;
    }

    /// <summary>Lista las vacunas del catalogo.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(List<VacunaRespuestaDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<VacunaRespuestaDto>>> Listar()
    {
        return Ok(await _vacunacion.ListarVacunasAsync());
    }

    /// <summary>Agrega una vacuna al catalogo. Roles: Administrador y Veterinario.</summary>
    [HttpPost]
    [Authorize(Roles = RolesEscritura)]
    [ProducesResponseType(typeof(VacunaRespuestaDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(RespuestaError), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(RespuestaError), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(RespuestaError), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<VacunaRespuestaDto>> Crear([FromBody] VacunaSolicitudDto dto)
    {
        var vacuna = await _vacunacion.CrearVacunaAsync(dto);
        return StatusCode(StatusCodes.Status201Created, vacuna);
    }

    /// <summary>Actualiza una vacuna. Roles: Administrador y Veterinario.</summary>
    [HttpPut("{id:int}")]
    [Authorize(Roles = RolesEscritura)]
    [ProducesResponseType(typeof(VacunaRespuestaDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(RespuestaError), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(RespuestaError), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(RespuestaError), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(RespuestaError), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<VacunaRespuestaDto>> Actualizar(int id, [FromBody] VacunaSolicitudDto dto)
    {
        return Ok(await _vacunacion.ActualizarVacunaAsync(id, dto));
    }

    /// <summary>Elimina una vacuna que nunca se aplico. Rol: Administrador.</summary>
    [HttpDelete("{id:int}")]
    [Authorize(Roles = NombresRoles.Administrador)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(RespuestaError), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(RespuestaError), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(RespuestaError), StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> Eliminar(int id)
    {
        await _vacunacion.EliminarVacunaAsync(id);
        return NoContent();
    }
}
