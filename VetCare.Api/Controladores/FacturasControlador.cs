using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VetCare.Api.Modelos;
using VetCare.Negocio.Constantes;
using VetCare.Negocio.Dtos;
using VetCare.Negocio.Servicios;

namespace VetCare.Api.Controladores;

/// <summary>Facturacion a clientes.</summary>
[ApiController]
[Route("api/facturas")]
[Produces("application/json")]
[Authorize]
[ProducesResponseType(typeof(RespuestaError), StatusCodes.Status401Unauthorized)]
public class FacturasControlador : ControllerBase
{
    private const string RolesEscritura = NombresRoles.Administrador + "," + NombresRoles.Recepcionista;

    private readonly IServicioFactura _facturas;

    public FacturasControlador(IServicioFactura facturas)
    {
        _facturas = facturas;
    }

    /// <summary>Lista las facturas, de la mas reciente a la mas antigua. Filtros opcionales: clienteId y estado.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(List<FacturaRespuestaDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<FacturaRespuestaDto>>> Listar([FromQuery] int? clienteId, [FromQuery] string? estado)
    {
        return Ok(await _facturas.ListarAsync(clienteId, estado));
    }

    /// <summary>Obtiene una factura con sus lineas.</summary>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(FacturaRespuestaDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(RespuestaError), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<FacturaRespuestaDto>> Obtener(int id)
    {
        return Ok(await _facturas.ObtenerAsync(id));
    }

    /// <summary>Emite una factura pendiente y descuenta del inventario los productos vendidos. Roles: Administrador y Recepcionista.</summary>
    [HttpPost]
    [Authorize(Roles = RolesEscritura)]
    [ProducesResponseType(typeof(FacturaRespuestaDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(RespuestaError), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(RespuestaError), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(RespuestaError), StatusCodes.Status422UnprocessableEntity)]
    public async Task<ActionResult<FacturaRespuestaDto>> Emitir([FromBody] FacturaSolicitudDto dto)
    {
        var factura = await _facturas.EmitirAsync(dto);
        return CreatedAtAction(nameof(Obtener), new { id = factura.Id }, factura);
    }

    /// <summary>Marca una factura pendiente como pagada. Roles: Administrador y Recepcionista.</summary>
    [HttpPost("{id:int}/pagar")]
    [Authorize(Roles = RolesEscritura)]
    [ProducesResponseType(typeof(FacturaRespuestaDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(RespuestaError), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(RespuestaError), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(RespuestaError), StatusCodes.Status409Conflict)]
    [ProducesResponseType(typeof(RespuestaError), StatusCodes.Status422UnprocessableEntity)]
    public async Task<ActionResult<FacturaRespuestaDto>> Pagar(int id)
    {
        return Ok(await _facturas.PagarAsync(id));
    }

    /// <summary>Anula una factura y devuelve los productos al inventario. Rol: Administrador.</summary>
    [HttpPost("{id:int}/anular")]
    [Authorize(Roles = NombresRoles.Administrador)]
    [ProducesResponseType(typeof(FacturaRespuestaDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(RespuestaError), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(RespuestaError), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(RespuestaError), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<FacturaRespuestaDto>> Anular(int id)
    {
        return Ok(await _facturas.AnularAsync(id));
    }
}
