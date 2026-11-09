using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VetCare.Api.Modelos;
using VetCare.Negocio.Constantes;
using VetCare.Negocio.Dtos;
using VetCare.Negocio.Servicios;

namespace VetCare.Api.Controladores;

/// <summary>Inventario de medicamentos usados en los tratamientos.</summary>
[ApiController]
[Route("api/medicamentos")]
[Produces("application/json")]
[Authorize]
[ProducesResponseType(typeof(RespuestaError), StatusCodes.Status401Unauthorized)]
public class MedicamentosControlador : ControllerBase
{
    private const string RolesEscritura = NombresRoles.Administrador + "," + NombresRoles.Veterinario;

    private readonly IServicioInventario _inventario;

    public MedicamentosControlador(IServicioInventario inventario)
    {
        _inventario = inventario;
    }

    /// <summary>Lista los medicamentos. Con busqueda filtra por nombre.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(List<MedicamentoRespuestaDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<MedicamentoRespuestaDto>>> Listar([FromQuery] string? busqueda)
    {
        return Ok(await _inventario.ListarMedicamentosAsync(busqueda));
    }

    /// <summary>Obtiene un medicamento por su id.</summary>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(MedicamentoRespuestaDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(RespuestaError), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<MedicamentoRespuestaDto>> Obtener(int id)
    {
        return Ok(await _inventario.ObtenerMedicamentoAsync(id));
    }

    /// <summary>Agrega un medicamento. Roles: Administrador y Veterinario.</summary>
    [HttpPost]
    [Authorize(Roles = RolesEscritura)]
    [ProducesResponseType(typeof(MedicamentoRespuestaDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(RespuestaError), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(RespuestaError), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(RespuestaError), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<MedicamentoRespuestaDto>> Crear([FromBody] MedicamentoSolicitudDto dto)
    {
        var medicamento = await _inventario.CrearMedicamentoAsync(dto);
        return CreatedAtAction(nameof(Obtener), new { id = medicamento.Id }, medicamento);
    }

    /// <summary>Actualiza un medicamento. Roles: Administrador y Veterinario.</summary>
    [HttpPut("{id:int}")]
    [Authorize(Roles = RolesEscritura)]
    [ProducesResponseType(typeof(MedicamentoRespuestaDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(RespuestaError), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(RespuestaError), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(RespuestaError), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(RespuestaError), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<MedicamentoRespuestaDto>> Actualizar(int id, [FromBody] MedicamentoSolicitudDto dto)
    {
        return Ok(await _inventario.ActualizarMedicamentoAsync(id, dto));
    }

    /// <summary>Elimina un medicamento que nunca se uso. Rol: Administrador.</summary>
    [HttpDelete("{id:int}")]
    [Authorize(Roles = NombresRoles.Administrador)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(RespuestaError), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(RespuestaError), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(RespuestaError), StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> Eliminar(int id)
    {
        await _inventario.EliminarMedicamentoAsync(id);
        return NoContent();
    }
}

/// <summary>Inventario de productos a la venta (alimentos, accesorios, higiene).</summary>
[ApiController]
[Route("api/productos")]
[Produces("application/json")]
[Authorize]
[ProducesResponseType(typeof(RespuestaError), StatusCodes.Status401Unauthorized)]
public class ProductosControlador : ControllerBase
{
    private const string RolesEscritura = NombresRoles.Administrador + "," + NombresRoles.Recepcionista;

    private readonly IServicioInventario _inventario;

    public ProductosControlador(IServicioInventario inventario)
    {
        _inventario = inventario;
    }

    /// <summary>Lista los productos. Con busqueda filtra por nombre.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(List<ProductoRespuestaDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<ProductoRespuestaDto>>> Listar([FromQuery] string? busqueda)
    {
        return Ok(await _inventario.ListarProductosAsync(busqueda));
    }

    /// <summary>Obtiene un producto por su id.</summary>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(ProductoRespuestaDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(RespuestaError), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ProductoRespuestaDto>> Obtener(int id)
    {
        return Ok(await _inventario.ObtenerProductoAsync(id));
    }

    /// <summary>Agrega un producto. Roles: Administrador y Recepcionista.</summary>
    [HttpPost]
    [Authorize(Roles = RolesEscritura)]
    [ProducesResponseType(typeof(ProductoRespuestaDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(RespuestaError), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(RespuestaError), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(RespuestaError), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<ProductoRespuestaDto>> Crear([FromBody] ProductoSolicitudDto dto)
    {
        var producto = await _inventario.CrearProductoAsync(dto);
        return CreatedAtAction(nameof(Obtener), new { id = producto.Id }, producto);
    }

    /// <summary>Actualiza un producto. Roles: Administrador y Recepcionista.</summary>
    [HttpPut("{id:int}")]
    [Authorize(Roles = RolesEscritura)]
    [ProducesResponseType(typeof(ProductoRespuestaDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(RespuestaError), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(RespuestaError), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(RespuestaError), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(RespuestaError), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<ProductoRespuestaDto>> Actualizar(int id, [FromBody] ProductoSolicitudDto dto)
    {
        return Ok(await _inventario.ActualizarProductoAsync(id, dto));
    }

    /// <summary>Elimina un producto que nunca se facturo. Rol: Administrador.</summary>
    [HttpDelete("{id:int}")]
    [Authorize(Roles = NombresRoles.Administrador)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(RespuestaError), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(RespuestaError), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(RespuestaError), StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> Eliminar(int id)
    {
        await _inventario.EliminarProductoAsync(id);
        return NoContent();
    }
}
