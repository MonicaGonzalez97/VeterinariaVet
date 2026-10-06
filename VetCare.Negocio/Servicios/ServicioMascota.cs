using VetCare.Datos.Entidades;
using VetCare.Datos.Repositorios;
using VetCare.Negocio.Dtos;
using VetCare.Negocio.Excepciones;

namespace VetCare.Negocio.Servicios;

public class ServicioMascota : IServicioMascota
{
    private readonly IRepositorioMascota _mascotas;
    private readonly IRepositorioCliente _clientes;
    private readonly IRepositorioRaza _razas;
    private readonly IRepositorio<Especie> _especies;

    public ServicioMascota(
        IRepositorioMascota mascotas,
        IRepositorioCliente clientes,
        IRepositorioRaza razas,
        IRepositorio<Especie> especies)
    {
        _mascotas = mascotas;
        _clientes = clientes;
        _razas = razas;
        _especies = especies;
    }

    public async Task<List<MascotaRespuestaDto>> ListarAsync(int? clienteId, bool fallecidas)
    {
        var mascotas = await _mascotas.ListarConDetalleAsync(clienteId, fallecidas);
        return mascotas.Select(Mapear).ToList();
    }

    public async Task<MascotaRespuestaDto> ObtenerAsync(int id)
    {
        var mascota = await ObtenerOFallarAsync(id);
        return Mapear(mascota);
    }

    public async Task<MascotaRespuestaDto> CrearAsync(MascotaSolicitudDto dto)
    {
        var cliente = await _clientes.ObtenerPorIdAsync(dto.ClienteId)
            ?? throw new ExcepcionReglaNegocio("El cliente indicado no existe");
        var raza = await _razas.ObtenerPorIdAsync(dto.RazaId)
            ?? throw new ExcepcionReglaNegocio("La raza indicada no existe");

        ValidarFechaNacimiento(dto.FechaNacimiento);

        if (await _mascotas.ExisteNombreParaClienteAsync(cliente.Id, dto.Nombre, null))
        {
            throw new ExcepcionConflicto("Este cliente ya tiene una mascota con ese nombre");
        }

        var mascota = new Mascota();
        Copiar(dto, mascota);
        await _mascotas.CrearAsync(mascota);

        var guardada = await _mascotas.ObtenerConDetalleAsync(mascota.Id) ?? mascota;
        return Mapear(guardada);
    }

    public async Task<MascotaRespuestaDto> ActualizarAsync(int id, MascotaSolicitudDto dto)
    {
        var mascota = await ObtenerOFallarAsync(id);

        if (mascota.Fallecida)
        {
            throw new ExcepcionReglaNegocio("No se puede modificar una mascota fallecida");
        }

        var cliente = await _clientes.ObtenerPorIdAsync(dto.ClienteId)
            ?? throw new ExcepcionReglaNegocio("El cliente indicado no existe");
        var raza = await _razas.ObtenerPorIdAsync(dto.RazaId)
            ?? throw new ExcepcionReglaNegocio("La raza indicada no existe");

        ValidarFechaNacimiento(dto.FechaNacimiento);

        if (await _mascotas.ExisteNombreParaClienteAsync(cliente.Id, dto.Nombre, id))
        {
            throw new ExcepcionConflicto("Este cliente ya tiene una mascota con ese nombre");
        }

        Copiar(dto, mascota);
        mascota.Cliente = cliente;
        mascota.Raza = raza;
        await _mascotas.ActualizarAsync(mascota);

        var guardada = await _mascotas.ObtenerConDetalleAsync(id) ?? mascota;
        return Mapear(guardada);
    }

    public async Task EliminarAsync(int id)
    {
        var mascota = await ObtenerOFallarAsync(id);

        if (await _mascotas.TieneHistorialAsync(id))
        {
            throw new ExcepcionReglaNegocio(
                "No se puede eliminar una mascota con historial clinico. Si fallecio, registre su fallecimiento");
        }

        await _mascotas.EliminarAsync(mascota);
    }

    public async Task<MascotaRespuestaDto> RegistrarFallecimientoAsync(int id, FallecimientoSolicitudDto dto, int? usuarioId)
    {
        var mascota = await ObtenerOFallarAsync(id);

        if (mascota.Fallecida)
        {
            throw new ExcepcionConflicto("La mascota ya tiene un fallecimiento registrado");
        }

        var fecha = dto.FechaFallecimiento
            ?? throw new ExcepcionReglaNegocio("La fecha de fallecimiento es obligatoria");

        if (fecha > LimiteFechaHoy())
        {
            throw new ExcepcionReglaNegocio("La fecha de fallecimiento no puede ser futura");
        }

        if (mascota.FechaNacimiento.HasValue && fecha < mascota.FechaNacimiento.Value)
        {
            throw new ExcepcionReglaNegocio("La fecha de fallecimiento no puede ser anterior al nacimiento");
        }

        var registro = new RegistroFallecimiento
        {
            FechaFallecimiento = fecha,
            Causa = string.IsNullOrWhiteSpace(dto.Causa) ? null : dto.Causa.Trim(),
            Observaciones = string.IsNullOrWhiteSpace(dto.Observaciones) ? null : dto.Observaciones.Trim(),
            UsuarioRegistroId = usuarioId
        };

        await _mascotas.RegistrarFallecimientoAsync(mascota, registro);
        return Mapear(mascota);
    }

    public async Task<MascotaRespuestaDto> RevertirFallecimientoAsync(int id)
    {
        var mascota = await ObtenerOFallarAsync(id);

        if (!mascota.Fallecida)
        {
            throw new ExcepcionReglaNegocio("La mascota no tiene un fallecimiento registrado");
        }

        await _mascotas.RevertirFallecimientoAsync(mascota);
        return Mapear(mascota);
    }

    public async Task<List<EspecieDto>> ListarEspeciesAsync()
    {
        var especies = await _especies.ListarAsync();
        return especies
            .OrderBy(e => e.Nombre)
            .Select(e => new EspecieDto { Id = e.Id, Nombre = e.Nombre })
            .ToList();
    }

    public async Task<List<RazaDto>> ListarRazasAsync(int? especieId)
    {
        var razas = await _razas.ListarPorEspecieAsync(especieId);
        return razas
            .Select(r => new RazaDto
            {
                Id = r.Id,
                Nombre = r.Nombre,
                EspecieId = r.EspecieId,
                EspecieNombre = r.Especie?.Nombre ?? string.Empty
            })
            .ToList();
    }

    private async Task<Mascota> ObtenerOFallarAsync(int id)
    {
        return await _mascotas.ObtenerConDetalleAsync(id)
            ?? throw new ExcepcionNoEncontrada($"No existe una mascota con id {id}");
    }

    // Se permite un dia de margen para no rechazar fechas "de hoy" por diferencia de zona horaria
    private static DateOnly LimiteFechaHoy()
    {
        return DateOnly.FromDateTime(DateTime.UtcNow).AddDays(1);
    }

    private static void ValidarFechaNacimiento(DateOnly? fechaNacimiento)
    {
        if (fechaNacimiento.HasValue && fechaNacimiento.Value > LimiteFechaHoy())
        {
            throw new ExcepcionReglaNegocio("La fecha de nacimiento no puede ser futura");
        }
    }

    private static void Copiar(MascotaSolicitudDto dto, Mascota mascota)
    {
        mascota.Nombre = dto.Nombre.Trim();
        mascota.ClienteId = dto.ClienteId;
        mascota.RazaId = dto.RazaId;
        mascota.FechaNacimiento = dto.FechaNacimiento;
        mascota.Sexo = dto.Sexo;
        mascota.PesoKg = dto.PesoKg;
        mascota.Color = string.IsNullOrWhiteSpace(dto.Color) ? null : dto.Color.Trim();
    }

    private static MascotaRespuestaDto Mapear(Mascota mascota)
    {
        return new MascotaRespuestaDto
        {
            Id = mascota.Id,
            Nombre = mascota.Nombre,
            ClienteId = mascota.ClienteId,
            ClienteNombre = mascota.Cliente == null
                ? string.Empty
                : $"{mascota.Cliente.Nombre} {mascota.Cliente.Apellido}",
            RazaId = mascota.RazaId,
            RazaNombre = mascota.Raza?.Nombre ?? string.Empty,
            EspecieNombre = mascota.Raza?.Especie?.Nombre ?? string.Empty,
            FechaNacimiento = mascota.FechaNacimiento,
            Sexo = mascota.Sexo,
            PesoKg = mascota.PesoKg,
            Color = mascota.Color,
            Fallecida = mascota.Fallecida,
            Fallecimiento = mascota.RegistroFallecimiento == null
                ? null
                : new FallecimientoRespuestaDto
                {
                    FechaFallecimiento = mascota.RegistroFallecimiento.FechaFallecimiento,
                    Causa = mascota.RegistroFallecimiento.Causa,
                    Observaciones = mascota.RegistroFallecimiento.Observaciones
                }
        };
    }
}
