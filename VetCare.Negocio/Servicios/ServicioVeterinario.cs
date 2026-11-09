using VetCare.Datos.Entidades;
using VetCare.Datos.Repositorios;
using VetCare.Negocio.Dtos;
using VetCare.Negocio.Excepciones;

namespace VetCare.Negocio.Servicios;

public interface IServicioVeterinario
{
    Task<List<VeterinarioRespuestaDto>> ListarAsync();
    Task<VeterinarioRespuestaDto> ObtenerAsync(int id);
    Task<VeterinarioRespuestaDto> CrearAsync(VeterinarioSolicitudDto dto);
    Task<VeterinarioRespuestaDto> ActualizarAsync(int id, VeterinarioSolicitudDto dto);
    Task EliminarAsync(int id);
}

public class ServicioVeterinario : IServicioVeterinario
{
    private readonly IRepositorioVeterinario _veterinarios;

    public ServicioVeterinario(IRepositorioVeterinario veterinarios)
    {
        _veterinarios = veterinarios;
    }

    public async Task<List<VeterinarioRespuestaDto>> ListarAsync()
    {
        var lista = await _veterinarios.ListarAsync();
        return lista.Select(Mapear).ToList();
    }

    public async Task<VeterinarioRespuestaDto> ObtenerAsync(int id)
    {
        var veterinario = await _veterinarios.ObtenerPorIdAsync(id)
            ?? throw new ExcepcionNoEncontrada($"No existe un veterinario con id {id}");
        return Mapear(veterinario);
    }

    public async Task<VeterinarioRespuestaDto> CrearAsync(VeterinarioSolicitudDto dto)
    {
        if (await _veterinarios.BuscarPorColegiadoAsync(dto.NumeroColegiado.Trim()) != null)
        {
            throw new ExcepcionConflicto("Ya existe un veterinario con ese numero de colegiado");
        }

        var veterinario = new Veterinario();
        Copiar(dto, veterinario);
        await _veterinarios.CrearAsync(veterinario);
        return Mapear(veterinario);
    }

    public async Task<VeterinarioRespuestaDto> ActualizarAsync(int id, VeterinarioSolicitudDto dto)
    {
        var veterinario = await _veterinarios.ObtenerPorIdAsync(id)
            ?? throw new ExcepcionNoEncontrada($"No existe un veterinario con id {id}");

        var otro = await _veterinarios.BuscarPorColegiadoAsync(dto.NumeroColegiado.Trim());
        if (otro != null && otro.Id != id)
        {
            throw new ExcepcionConflicto("Otro veterinario ya usa ese numero de colegiado");
        }

        Copiar(dto, veterinario);
        await _veterinarios.ActualizarAsync(veterinario);
        return Mapear(veterinario);
    }

    public async Task EliminarAsync(int id)
    {
        var veterinario = await _veterinarios.ObtenerPorIdAsync(id)
            ?? throw new ExcepcionNoEncontrada($"No existe un veterinario con id {id}");

        if (await _veterinarios.TieneActividadAsync(id))
        {
            throw new ExcepcionReglaNegocio("No se puede eliminar un veterinario con citas, consultas o vacunas registradas");
        }

        await _veterinarios.EliminarAsync(veterinario);
    }

    private static void Copiar(VeterinarioSolicitudDto dto, Veterinario veterinario)
    {
        veterinario.Nombre = dto.Nombre.Trim();
        veterinario.Apellido = dto.Apellido.Trim();
        veterinario.NumeroColegiado = dto.NumeroColegiado.Trim();
        veterinario.Especialidad = string.IsNullOrWhiteSpace(dto.Especialidad) ? null : dto.Especialidad.Trim();
    }

    private static VeterinarioRespuestaDto Mapear(Veterinario veterinario)
    {
        return new VeterinarioRespuestaDto
        {
            Id = veterinario.Id,
            Nombre = veterinario.Nombre,
            Apellido = veterinario.Apellido,
            NombreCompleto = $"{veterinario.Nombre} {veterinario.Apellido}",
            NumeroColegiado = veterinario.NumeroColegiado,
            Especialidad = veterinario.Especialidad
        };
    }
}
