using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VetCare.Api.Modelos;
using VetCare.Negocio.Constantes;
using VetCare.Negocio.Dtos;
using VetCare.Negocio.Servicios;

namespace VetCare.Api.Controladores;

/// <summary>Gestion de clientes (dueños de las mascotas).</summary>
[ApiController]
[Route("api/clientes")]
[Produces("application/json")]
[Authorize]
public class ClientesControlador : ControllerBase
{
    private readonly IServicioCliente _clientes;

    public ClientesControlador(IServicioCliente clientes)
    {
        _clientes = clientes;
    }

    /// <summary>Lista los clientes. Con el parametro busqueda filtra por nombre, apellido o documento.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(List<ClienteRespuestaDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(RespuestaError), StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<List<ClienteRespuestaDto>>> Listar([FromQuery] string? busqueda)
    {
        return Ok(await _clientes.ListarAsync(busqueda));
    }

    /// <summary>Obtiene un cliente por su id.</summary>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(ClienteRespuestaDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(RespuestaError), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ClienteRespuestaDto>> Obtener(int id)
    {
        return Ok(await _clientes.ObtenerAsync(id));
    }

    /// <summary>Crea un cliente. Roles: Administrador y Recepcionista.</summary>
    [HttpPost]
    [Authorize(Roles = NombresRoles.Administrador + "," + NombresRoles.Recepcionista)]
    [ProducesResponseType(typeof(ClienteRespuestaDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(RespuestaError), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(RespuestaError), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(RespuestaError), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<ClienteRespuestaDto>> Crear([FromBody] ClienteSolicitudDto dto)
    {
        var cliente = await _clientes.CrearAsync(dto);
        return CreatedAtAction(nameof(Obtener), new { id = cliente.Id }, cliente);
    }

    /// <summary>Actualiza un cliente. Roles: Administrador y Recepcionista.</summary>
    [HttpPut("{id:int}")]
    [Authorize(Roles = NombresRoles.Administrador + "," + NombresRoles.Recepcionista)]
    [ProducesResponseType(typeof(ClienteRespuestaDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(RespuestaError), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(RespuestaError), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(RespuestaError), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(RespuestaError), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<ClienteRespuestaDto>> Actualizar(int id, [FromBody] ClienteSolicitudDto dto)
    {
        return Ok(await _clientes.ActualizarAsync(id, dto));
    }

    /// <summary>Elimina un cliente sin mascotas. Rol: Administrador.</summary>
    [HttpDelete("{id:int}")]
    [Authorize(Roles = NombresRoles.Administrador)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(RespuestaError), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(RespuestaError), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(RespuestaError), StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> Eliminar(int id)
    {
        await _clientes.EliminarAsync(id);
        return NoContent();
    }
}
