using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VetCare.Api.Modelos;
using VetCare.Negocio.Constantes;
using VetCare.Negocio.Dtos;
using VetCare.Negocio.Servicios;

namespace VetCare.Api.Controladores;

/// <summary>Agenda de citas de la clinica.</summary>
[ApiController]
[Route("api/citas")]
[Produces("application/json")]
[Authorize]
[ProducesResponseType(typeof(RespuestaError), StatusCodes.Status401Unauthorized)]
public class CitasControlador : ControllerBase
{
    private const string RolesEscritura = NombresRoles.Administrador + "," + NombresRoles.Recepcionista;

    private readonly IServicioCita _citas;

    public CitasControlador(IServicioCita citas)
    {
        _citas = citas;
    }

    /// <summary>Lista las citas ordenadas por fecha. Filtros opcionales: fecha (aaaa-mm-dd), veterinarioId, mascotaId y estado.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(List<CitaRespuestaDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<CitaRespuestaDto>>> Listar(
        [FromQuery] DateOnly? fecha,
        [FromQuery] int? veterinarioId,
        [FromQuery] int? mascotaId,
        [FromQuery] string? estado)
    {
        return Ok(await _citas.ListarAsync(fecha, veterinarioId, mascotaId, estado));
    }

    /// <summary>Obtiene una cita por su id.</summary>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(CitaRespuestaDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(RespuestaError), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<CitaRespuestaDto>> Obtener(int id)
    {
        return Ok(await _citas.ObtenerAsync(id));
    }

    /// <summary>Programa una cita. Roles: Administrador y Recepcionista.</summary>
    [HttpPost]
    [Authorize(Roles = RolesEscritura)]
    [ProducesResponseType(typeof(CitaRespuestaDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(RespuestaError), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(RespuestaError), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(RespuestaError), StatusCodes.Status409Conflict)]
    [ProducesResponseType(typeof(RespuestaError), StatusCodes.Status422UnprocessableEntity)]
    public async Task<ActionResult<CitaRespuestaDto>> Crear([FromBody] CitaSolicitudDto dto)
    {
        var cita = await _citas.CrearAsync(dto);
        return CreatedAtAction(nameof(Obtener), new { id = cita.Id }, cita);
    }

    /// <summary>Reprograma una cita programada. Roles: Administrador y Recepcionista.</summary>
    [HttpPut("{id:int}")]
    [Authorize(Roles = RolesEscritura)]
    [ProducesResponseType(typeof(CitaRespuestaDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(RespuestaError), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(RespuestaError), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(RespuestaError), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(RespuestaError), StatusCodes.Status409Conflict)]
    [ProducesResponseType(typeof(RespuestaError), StatusCodes.Status422UnprocessableEntity)]
    public async Task<ActionResult<CitaRespuestaDto>> Actualizar(int id, [FromBody] CitaSolicitudDto dto)
    {
        return Ok(await _citas.ActualizarAsync(id, dto));
    }

    /// <summary>Cancela una cita programada. Roles: Administrador y Recepcionista.</summary>
    [HttpPost("{id:int}/cancelar")]
    [Authorize(Roles = RolesEscritura)]
    [ProducesResponseType(typeof(CitaRespuestaDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(RespuestaError), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(RespuestaError), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(RespuestaError), StatusCodes.Status409Conflict)]
    [ProducesResponseType(typeof(RespuestaError), StatusCodes.Status422UnprocessableEntity)]
    public async Task<ActionResult<CitaRespuestaDto>> Cancelar(int id)
    {
        return Ok(await _citas.CancelarAsync(id));
    }
}
