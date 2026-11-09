using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VetCare.Api.Modelos;
using VetCare.Negocio.Constantes;
using VetCare.Negocio.Dtos;
using VetCare.Negocio.Servicios;

namespace VetCare.Api.Controladores;

/// <summary>Consultas medicas con sus diagnosticos y tratamientos (historial clinico).</summary>
[ApiController]
[Route("api/consultas")]
[Produces("application/json")]
[Authorize]
[ProducesResponseType(typeof(RespuestaError), StatusCodes.Status401Unauthorized)]
public class ConsultasControlador : ControllerBase
{
    private readonly IServicioConsulta _consultas;

    public ConsultasControlador(IServicioConsulta consultas)
    {
        _consultas = consultas;
    }

    /// <summary>Lista las consultas. Con mascotaId devuelve el historial clinico de una mascota.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(List<ConsultaRespuestaDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<ConsultaRespuestaDto>>> Listar([FromQuery] int? mascotaId)
    {
        return Ok(await _consultas.ListarAsync(mascotaId));
    }

    /// <summary>Obtiene una consulta con sus diagnosticos y tratamientos.</summary>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(ConsultaRespuestaDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(RespuestaError), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ConsultaRespuestaDto>> Obtener(int id)
    {
        return Ok(await _consultas.ObtenerAsync(id));
    }

    /// <summary>
    /// Registra una consulta. Descuenta existencias de los medicamentos, actualiza el peso de la mascota
    /// y marca como completada la cita atendida. Roles: Administrador y Veterinario.
    /// </summary>
    [HttpPost]
    [Authorize(Roles = NombresRoles.Administrador + "," + NombresRoles.Veterinario)]
    [ProducesResponseType(typeof(ConsultaRespuestaDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(RespuestaError), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(RespuestaError), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(RespuestaError), StatusCodes.Status422UnprocessableEntity)]
    public async Task<ActionResult<ConsultaRespuestaDto>> Registrar([FromBody] ConsultaSolicitudDto dto)
    {
        var consulta = await _consultas.RegistrarAsync(dto);
        return CreatedAtAction(nameof(Obtener), new { id = consulta.Id }, consulta);
    }
}
