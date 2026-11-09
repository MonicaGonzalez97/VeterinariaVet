using VetCare.Datos.Entidades;
using VetCare.Datos.Repositorios;
using VetCare.Negocio.Constantes;
using VetCare.Negocio.Dtos;
using VetCare.Negocio.Excepciones;
using VetCare.Negocio.Utilidades;

namespace VetCare.Negocio.Servicios;

public interface IServicioCita
{
    Task<List<CitaRespuestaDto>> ListarAsync(DateOnly? fecha, int? veterinarioId, int? mascotaId, string? estado);
    Task<CitaRespuestaDto> ObtenerAsync(int id);
    Task<CitaRespuestaDto> CrearAsync(CitaSolicitudDto dto);
    Task<CitaRespuestaDto> ActualizarAsync(int id, CitaSolicitudDto dto);
    Task<CitaRespuestaDto> CancelarAsync(int id);
}

public class ServicioCita : IServicioCita
{
    private readonly IRepositorioCita _citas;
    private readonly IRepositorioMascota _mascotas;
    private readonly IRepositorioVeterinario _veterinarios;

    public ServicioCita(IRepositorioCita citas, IRepositorioMascota mascotas, IRepositorioVeterinario veterinarios)
    {
        _citas = citas;
        _mascotas = mascotas;
        _veterinarios = veterinarios;
    }

    public async Task<List<CitaRespuestaDto>> ListarAsync(DateOnly? fecha, int? veterinarioId, int? mascotaId, string? estado)
    {
        var lista = await _citas.BuscarAsync(fecha, veterinarioId, mascotaId, estado);
        return lista.Select(Mapear).ToList();
    }

    public async Task<CitaRespuestaDto> ObtenerAsync(int id)
    {
        var cita = await _citas.ObtenerConDetalleAsync(id)
            ?? throw new ExcepcionNoEncontrada($"No existe una cita con id {id}");
        return Mapear(cita);
    }

    public async Task<CitaRespuestaDto> CrearAsync(CitaSolicitudDto dto)
    {
        var fechaHora = ValidarFechaHora(dto.FechaHora);
        var mascota = await ObtenerMascotaActivaAsync(dto.MascotaId);
        var veterinario = await _veterinarios.ObtenerPorIdAsync(dto.VeterinarioId)
            ?? throw new ExcepcionReglaNegocio("El veterinario indicado no existe");

        if (await _citas.ExisteCruceAsync(veterinario.Id, fechaHora, null))
        {
            throw new ExcepcionConflicto("El veterinario ya tiene una cita en ese horario (margen de 30 minutos)");
        }

        var cita = new Cita
        {
            MascotaId = mascota.Id,
            Mascota = mascota,
            VeterinarioId = veterinario.Id,
            Veterinario = veterinario,
            FechaHora = fechaHora,
            Motivo = dto.Motivo.Trim(),
            Estado = EstadosCita.Programada
        };

        await _citas.CrearAsync(cita);
        var guardada = await _citas.ObtenerConDetalleAsync(cita.Id) ?? cita;
        return Mapear(guardada);
    }

    public async Task<CitaRespuestaDto> ActualizarAsync(int id, CitaSolicitudDto dto)
    {
        var cita = await _citas.ObtenerConDetalleAsync(id)
            ?? throw new ExcepcionNoEncontrada($"No existe una cita con id {id}");

        if (cita.Estado != EstadosCita.Programada)
        {
            throw new ExcepcionReglaNegocio("Solo se pueden modificar citas programadas");
        }

        var fechaHora = ValidarFechaHora(dto.FechaHora);
        var mascota = await ObtenerMascotaActivaAsync(dto.MascotaId);
        var veterinario = await _veterinarios.ObtenerPorIdAsync(dto.VeterinarioId)
            ?? throw new ExcepcionReglaNegocio("El veterinario indicado no existe");

        if (await _citas.ExisteCruceAsync(veterinario.Id, fechaHora, id))
        {
            throw new ExcepcionConflicto("El veterinario ya tiene una cita en ese horario (margen de 30 minutos)");
        }

        cita.MascotaId = mascota.Id;
        cita.Mascota = mascota;
        cita.VeterinarioId = veterinario.Id;
        cita.Veterinario = veterinario;
        cita.FechaHora = fechaHora;
        cita.Motivo = dto.Motivo.Trim();

        await _citas.ActualizarAsync(cita);
        var guardada = await _citas.ObtenerConDetalleAsync(id) ?? cita;
        return Mapear(guardada);
    }

    public async Task<CitaRespuestaDto> CancelarAsync(int id)
    {
        var cita = await _citas.ObtenerConDetalleAsync(id)
            ?? throw new ExcepcionNoEncontrada($"No existe una cita con id {id}");

        if (cita.Estado == EstadosCita.Cancelada)
        {
            throw new ExcepcionConflicto("La cita ya estaba cancelada");
        }

        if (cita.Estado == EstadosCita.Completada)
        {
            throw new ExcepcionReglaNegocio("No se puede cancelar una cita que ya fue atendida");
        }

        cita.Estado = EstadosCita.Cancelada;
        await _citas.ActualizarAsync(cita);
        return Mapear(cita);
    }

    private static DateTime ValidarFechaHora(DateTime? fechaHora)
    {
        if (!fechaHora.HasValue)
        {
            throw new ExcepcionReglaNegocio("La fecha y hora de la cita son obligatorias");
        }

        var utc = FechasClinica.ComoUtc(fechaHora.Value);
        if (utc < FechasClinica.InicioDeHoy())
        {
            throw new ExcepcionReglaNegocio("La cita no puede programarse en una fecha pasada");
        }

        return utc;
    }

    private async Task<Mascota> ObtenerMascotaActivaAsync(int mascotaId)
    {
        var mascota = await _mascotas.ObtenerPorIdAsync(mascotaId)
            ?? throw new ExcepcionReglaNegocio("La mascota indicada no existe");

        if (mascota.Fallecida)
        {
            throw new ExcepcionReglaNegocio("No se pueden programar citas para una mascota fallecida");
        }

        return mascota;
    }

    private static CitaRespuestaDto Mapear(Cita cita)
    {
        return new CitaRespuestaDto
        {
            Id = cita.Id,
            MascotaId = cita.MascotaId,
            MascotaNombre = cita.Mascota?.Nombre ?? string.Empty,
            ClienteNombre = cita.Mascota?.Cliente == null
                ? string.Empty
                : $"{cita.Mascota.Cliente.Nombre} {cita.Mascota.Cliente.Apellido}",
            VeterinarioId = cita.VeterinarioId,
            VeterinarioNombre = cita.Veterinario == null
                ? string.Empty
                : $"{cita.Veterinario.Nombre} {cita.Veterinario.Apellido}",
            FechaHora = cita.FechaHora,
            Motivo = cita.Motivo,
            Estado = cita.Estado
        };
    }
}
