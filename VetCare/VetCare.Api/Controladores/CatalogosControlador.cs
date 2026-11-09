using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VetCare.Negocio.Dtos;
using VetCare.Negocio.Servicios;

namespace VetCare.Api.Controladores;

/// <summary>Catalogos de solo lectura usados por los formularios (especies y razas).</summary>
[ApiController]
[Route("api/catalogos")]
[Produces("application/json")]
[Authorize]
public class CatalogosControlador : ControllerBase
{
    private readonly IServicioMascota _mascotas;

    public CatalogosControlador(IServicioMascota mascotas)
    {
        _mascotas = mascotas;
    }

    /// <summary>Lista las especies disponibles.</summary>
    [HttpGet("especies")]
    [ProducesResponseType(typeof(List<EspecieDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<EspecieDto>>> ListarEspecies()
    {
        return Ok(await _mascotas.ListarEspeciesAsync());
    }

    /// <summary>Lista las razas. Con especieId filtra por especie.</summary>
    [HttpGet("razas")]
    [ProducesResponseType(typeof(List<RazaDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<RazaDto>>> ListarRazas([FromQuery] int? especieId)
    {
        return Ok(await _mascotas.ListarRazasAsync(especieId));
    }
}
