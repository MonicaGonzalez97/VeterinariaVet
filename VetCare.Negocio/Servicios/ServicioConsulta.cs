using VetCare.Datos.Entidades;
using VetCare.Datos.Repositorios;
using VetCare.Negocio.Constantes;
using VetCare.Negocio.Dtos;
using VetCare.Negocio.Excepciones;
using VetCare.Negocio.Utilidades;

namespace VetCare.Negocio.Servicios;

public interface IServicioConsulta
{
    Task<List<ConsultaRespuestaDto>> ListarAsync(int? mascotaId);
    Task<ConsultaRespuestaDto> ObtenerAsync(int id);
    Task<ConsultaRespuestaDto> RegistrarAsync(ConsultaSolicitudDto dto);
}

public class ServicioConsulta : IServicioConsulta
{
    private readonly IRepositorioConsulta _consultas;
    private readonly IRepositorioMascota _mascotas;
    private readonly IRepositorioVeterinario _veterinarios;
    private readonly IRepositorioCita _citas;
    private readonly IRepositorioMedicamento _medicamentos;

    public ServicioConsulta(
        IRepositorioConsulta consultas,
        IRepositorioMascota mascotas,
        IRepositorioVeterinario veterinarios,
        IRepositorioCita citas,
        IRepositorioMedicamento medicamentos)
    {
        _consultas = consultas;
        _mascotas = mascotas;
        _veterinarios = veterinarios;
        _citas = citas;
        _medicamentos = medicamentos;
    }

    public async Task<List<ConsultaRespuestaDto>> ListarAsync(int? mascotaId)
    {
        var lista = await _consultas.ListarPorMascotaAsync(mascotaId);
        return lista.Select(Mapear).ToList();
    }

    public async Task<ConsultaRespuestaDto> ObtenerAsync(int id)
    {
        var consulta = await _consultas.ObtenerConDetalleAsync(id)
            ?? throw new ExcepcionNoEncontrada($"No existe una consulta con id {id}");
        return Mapear(consulta);
    }

    public async Task<ConsultaRespuestaDto> RegistrarAsync(ConsultaSolicitudDto dto)
    {
        var mascota = await _mascotas.ObtenerPorIdAsync(dto.MascotaId)
            ?? throw new ExcepcionReglaNegocio("La mascota indicada no existe");

        if (mascota.Fallecida)
        {
            throw new ExcepcionReglaNegocio("No se puede registrar una consulta para una mascota fallecida");
        }

        var veterinario = await _veterinarios.ObtenerPorIdAsync(dto.VeterinarioId)
            ?? throw new ExcepcionReglaNegocio("El veterinario indicado no existe");

        if (dto.Temperatura.HasValue && (dto.Temperatura.Value < 30m || dto.Temperatura.Value > 45m))
        {
            throw new ExcepcionReglaNegocio("La temperatura debe estar entre 30 y 45 grados Celsius");
        }

        var pesoKg = ConversorPeso.ObtenerKilogramos(dto.PesoKg, ConversorPeso.Kilogramos);

        Cita? cita = null;
        if (dto.CitaId.HasValue)
        {
            cita = await _citas.ObtenerPorIdAsync(dto.CitaId.Value)
                ?? throw new ExcepcionReglaNegocio("La cita indicada no existe");

            if (cita.MascotaId != dto.MascotaId || cita.VeterinarioId != dto.VeterinarioId)
            {
                throw new ExcepcionReglaNegocio("La cita no corresponde a la mascota y al veterinario indicados");
            }

            if (cita.Estado != EstadosCita.Programada)
            {
                throw new ExcepcionReglaNegocio("Solo se puede atender una cita programada");
            }
        }

        var consulta = new Consulta
        {
            Fecha = DateTime.UtcNow,
            PesoKg = pesoKg,
            Temperatura = dto.Temperatura,
            Observaciones = string.IsNullOrWhiteSpace(dto.Observaciones) ? null : dto.Observaciones.Trim(),
            CitaId = dto.CitaId,
            MascotaId = mascota.Id,
            Mascota = mascota,
            VeterinarioId = veterinario.Id,
            Veterinario = veterinario
        };

        foreach (var diagnostico in dto.Diagnosticos)
        {
            consulta.Diagnosticos.Add(new Diagnostico
            {
                Descripcion = diagnostico.Descripcion.Trim(),
                Gravedad = diagnostico.Gravedad
            });
        }

        foreach (var tratamiento in dto.Tratamientos)
        {
            Medicamento? medicamento = null;

            if (tratamiento.MedicamentoId.HasValue)
            {
                medicamento = await _medicamentos.ObtenerPorIdAsync(tratamiento.MedicamentoId.Value)
                    ?? throw new ExcepcionReglaNegocio("El medicamento indicado no existe");

                if (medicamento.Stock < 1)
                {
                    throw new ExcepcionReglaNegocio($"No hay existencias de {medicamento.Nombre}");
                }

                medicamento.Stock -= 1;
            }

            consulta.Tratamientos.Add(new Tratamiento
            {
                Indicaciones = tratamiento.Indicaciones.Trim(),
                Dosis = string.IsNullOrWhiteSpace(tratamiento.Dosis) ? null : tratamiento.Dosis.Trim(),
                DuracionDias = tratamiento.DuracionDias,
                MedicamentoId = medicamento?.Id,
                Medicamento = medicamento
            });
        }

        if (pesoKg.HasValue)
        {
            mascota.PesoKg = pesoKg;
        }

        if (cita != null)
        {
            cita.Estado = EstadosCita.Completada;
        }

        // Hay un DbContext por peticion: esta llamada guarda en una sola transaccion
        // la consulta, el descuento de existencias, el peso de la mascota y el estado de la cita.
        await _consultas.CrearAsync(consulta);

        var guardada = await _consultas.ObtenerConDetalleAsync(consulta.Id) ?? consulta;
        return Mapear(guardada);
    }

    private static ConsultaRespuestaDto Mapear(Consulta consulta)
    {
        return new ConsultaRespuestaDto
        {
            Id = consulta.Id,
            Fecha = consulta.Fecha,
            MascotaId = consulta.MascotaId,
            MascotaNombre = consulta.Mascota?.Nombre ?? string.Empty,
            VeterinarioId = consulta.VeterinarioId,
            VeterinarioNombre = consulta.Veterinario == null
                ? string.Empty
                : $"{consulta.Veterinario.Nombre} {consulta.Veterinario.Apellido}",
            CitaId = consulta.CitaId,
            PesoKg = consulta.PesoKg,
            Temperatura = consulta.Temperatura,
            Observaciones = consulta.Observaciones,
            Diagnosticos = consulta.Diagnosticos
                .Select(d => new DiagnosticoRespuestaDto { Id = d.Id, Descripcion = d.Descripcion, Gravedad = d.Gravedad })
                .ToList(),
            Tratamientos = consulta.Tratamientos
                .Select(t => new TratamientoRespuestaDto
                {
                    Id = t.Id,
                    MedicamentoId = t.MedicamentoId,
                    MedicamentoNombre = t.Medicamento?.Nombre,
                    Indicaciones = t.Indicaciones,
                    Dosis = t.Dosis,
                    DuracionDias = t.DuracionDias
                })
                .ToList()
        };
    }
}
