using VetCare.Datos.Entidades;
using VetCare.Datos.Repositorios;
using VetCare.Negocio.Dtos;
using VetCare.Negocio.Excepciones;
using VetCare.Negocio.Utilidades;

namespace VetCare.Negocio.Servicios;

public interface IServicioVacunacion
{
    Task<List<VacunaRespuestaDto>> ListarVacunasAsync();
    Task<VacunaRespuestaDto> CrearVacunaAsync(VacunaSolicitudDto dto);
    Task<VacunaRespuestaDto> ActualizarVacunaAsync(int id, VacunaSolicitudDto dto);
    Task EliminarVacunaAsync(int id);
    Task<List<RegistroVacunacionRespuestaDto>> ListarRegistrosAsync(int? mascotaId);
    Task<RegistroVacunacionRespuestaDto> RegistrarAplicacionAsync(RegistroVacunacionSolicitudDto dto);
    Task EliminarRegistroAsync(int id);
}

public class ServicioVacunacion : IServicioVacunacion
{
    private readonly IRepositorioVacuna _vacunas;
    private readonly IRepositorioRegistroVacunacion _registros;
    private readonly IRepositorioMascota _mascotas;
    private readonly IRepositorioVeterinario _veterinarios;

    public ServicioVacunacion(
        IRepositorioVacuna vacunas,
        IRepositorioRegistroVacunacion registros,
        IRepositorioMascota mascotas,
        IRepositorioVeterinario veterinarios)
    {
        _vacunas = vacunas;
        _registros = registros;
        _mascotas = mascotas;
        _veterinarios = veterinarios;
    }

    // ---------- Catalogo de vacunas ----------

    public async Task<List<VacunaRespuestaDto>> ListarVacunasAsync()
    {
        var lista = await _vacunas.ListarAsync();
        return lista.Select(MapearVacuna).ToList();
    }

    public async Task<VacunaRespuestaDto> CrearVacunaAsync(VacunaSolicitudDto dto)
    {
        if (await _vacunas.BuscarPorNombreAsync(dto.Nombre) != null)
        {
            throw new ExcepcionConflicto("Ya existe una vacuna con ese nombre");
        }

        var vacuna = new Vacuna { Nombre = dto.Nombre.Trim(), IntervaloDias = dto.IntervaloDias };
        await _vacunas.CrearAsync(vacuna);
        return MapearVacuna(vacuna);
    }

    public async Task<VacunaRespuestaDto> ActualizarVacunaAsync(int id, VacunaSolicitudDto dto)
    {
        var vacuna = await _vacunas.ObtenerPorIdAsync(id)
            ?? throw new ExcepcionNoEncontrada($"No existe una vacuna con id {id}");

        var otra = await _vacunas.BuscarPorNombreAsync(dto.Nombre);
        if (otra != null && otra.Id != id)
        {
            throw new ExcepcionConflicto("Ya existe otra vacuna con ese nombre");
        }

        vacuna.Nombre = dto.Nombre.Trim();
        vacuna.IntervaloDias = dto.IntervaloDias;
        await _vacunas.ActualizarAsync(vacuna);
        return MapearVacuna(vacuna);
    }

    public async Task EliminarVacunaAsync(int id)
    {
        var vacuna = await _vacunas.ObtenerPorIdAsync(id)
            ?? throw new ExcepcionNoEncontrada($"No existe una vacuna con id {id}");

        if (await _vacunas.TieneRegistrosAsync(id))
        {
            throw new ExcepcionReglaNegocio("No se puede eliminar una vacuna que ya fue aplicada a alguna mascota");
        }

        await _vacunas.EliminarAsync(vacuna);
    }

    // ---------- Carne de vacunacion ----------

    public async Task<List<RegistroVacunacionRespuestaDto>> ListarRegistrosAsync(int? mascotaId)
    {
        var lista = await _registros.ListarPorMascotaAsync(mascotaId);
        return lista.Select(MapearRegistro).ToList();
    }

    public async Task<RegistroVacunacionRespuestaDto> RegistrarAplicacionAsync(RegistroVacunacionSolicitudDto dto)
    {
        var mascota = await _mascotas.ObtenerPorIdAsync(dto.MascotaId)
            ?? throw new ExcepcionReglaNegocio("La mascota indicada no existe");

        if (mascota.Fallecida)
        {
            throw new ExcepcionReglaNegocio("No se puede vacunar a una mascota fallecida");
        }

        var vacuna = await _vacunas.ObtenerPorIdAsync(dto.VacunaId)
            ?? throw new ExcepcionReglaNegocio("La vacuna indicada no existe");
        var veterinario = await _veterinarios.ObtenerPorIdAsync(dto.VeterinarioId)
            ?? throw new ExcepcionReglaNegocio("El veterinario indicado no existe");

        var fecha = dto.FechaAplicacion
            ?? throw new ExcepcionReglaNegocio("La fecha de aplicacion es obligatoria");

        if (fecha > FechasClinica.LimiteFechaHoy())
        {
            throw new ExcepcionReglaNegocio("La fecha de aplicacion no puede ser futura");
        }

        var proxima = dto.ProximaDosis;
        if (proxima.HasValue && proxima.Value <= fecha)
        {
            throw new ExcepcionReglaNegocio("La proxima dosis debe ser posterior a la fecha de aplicacion");
        }

        if (!proxima.HasValue && vacuna.IntervaloDias > 0)
        {
            proxima = fecha.AddDays(vacuna.IntervaloDias);
        }

        var registro = new RegistroVacunacion
        {
            MascotaId = mascota.Id,
            Mascota = mascota,
            VacunaId = vacuna.Id,
            Vacuna = vacuna,
            VeterinarioId = veterinario.Id,
            Veterinario = veterinario,
            FechaAplicacion = fecha,
            ProximaDosis = proxima,
            Lote = string.IsNullOrWhiteSpace(dto.Lote) ? null : dto.Lote.Trim()
        };

        await _registros.CrearAsync(registro);
        return MapearRegistro(registro);
    }

    public async Task EliminarRegistroAsync(int id)
    {
        var registro = await _registros.ObtenerPorIdAsync(id)
            ?? throw new ExcepcionNoEncontrada($"No existe un registro de vacunacion con id {id}");
        await _registros.EliminarAsync(registro);
    }

    private static VacunaRespuestaDto MapearVacuna(Vacuna vacuna)
    {
        return new VacunaRespuestaDto { Id = vacuna.Id, Nombre = vacuna.Nombre, IntervaloDias = vacuna.IntervaloDias };
    }

    private static RegistroVacunacionRespuestaDto MapearRegistro(RegistroVacunacion registro)
    {
        return new RegistroVacunacionRespuestaDto
        {
            Id = registro.Id,
            MascotaId = registro.MascotaId,
            MascotaNombre = registro.Mascota?.Nombre ?? string.Empty,
            VacunaId = registro.VacunaId,
            VacunaNombre = registro.Vacuna?.Nombre ?? string.Empty,
            VeterinarioId = registro.VeterinarioId,
            VeterinarioNombre = registro.Veterinario == null
                ? string.Empty
                : $"{registro.Veterinario.Nombre} {registro.Veterinario.Apellido}",
            FechaAplicacion = registro.FechaAplicacion,
            ProximaDosis = registro.ProximaDosis,
            Lote = registro.Lote
        };
    }
}
