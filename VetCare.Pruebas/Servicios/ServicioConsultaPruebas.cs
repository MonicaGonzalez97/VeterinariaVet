using Moq;
using VetCare.Datos.Entidades;
using VetCare.Datos.Repositorios;
using VetCare.Negocio.Constantes;
using VetCare.Negocio.Dtos;
using VetCare.Negocio.Excepciones;
using VetCare.Negocio.Servicios;

namespace VetCare.Pruebas.Servicios;

public class ServicioConsultaPruebas
{
    private readonly Mock<IRepositorioConsulta> _consultas = new();
    private readonly Mock<IRepositorioMascota> _mascotas = new();
    private readonly Mock<IRepositorioVeterinario> _veterinarios = new();
    private readonly Mock<IRepositorioCita> _citas = new();
    private readonly Mock<IRepositorioMedicamento> _medicamentos = new();
    private readonly ServicioConsulta _servicio;

    public ServicioConsultaPruebas()
    {
        _servicio = new ServicioConsulta(
            _consultas.Object, _mascotas.Object, _veterinarios.Object, _citas.Object, _medicamentos.Object);

        _consultas.Setup(c => c.CrearAsync(It.IsAny<Consulta>()))
            .Callback<Consulta>(c => c.Id = 20)
            .ReturnsAsync((Consulta c) => c);
    }

    private static ConsultaSolicitudDto Solicitud() => new()
    {
        MascotaId = 1,
        VeterinarioId = 2,
        Observaciones = " Rasca las orejas ",
        Diagnosticos = new List<DiagnosticoSolicitudDto>
        {
            new() { Descripcion = "Otitis externa", Gravedad = "Leve" }
        }
    };

    private static Mascota MascotaActiva() => new() { Id = 1, Nombre = "Firulais", PesoKg = 10m };
    private static Veterinario UnVeterinario() => new() { Id = 2, Nombre = "Laura", Apellido = "Ramirez" };

    private Mascota ConfigurarBase()
    {
        var mascota = MascotaActiva();
        _mascotas.Setup(m => m.ObtenerPorIdAsync(1)).ReturnsAsync(mascota);
        _veterinarios.Setup(v => v.ObtenerPorIdAsync(2)).ReturnsAsync(UnVeterinario());
        return mascota;
    }

    [Fact]
    public async Task Registrar_MascotaFallecida_LanzaReglaNegocio()
    {
        var mascota = ConfigurarBase();
        mascota.Fallecida = true;

        await Assert.ThrowsAsync<ExcepcionReglaNegocio>(() => _servicio.RegistrarAsync(Solicitud()));
        _consultas.Verify(c => c.CrearAsync(It.IsAny<Consulta>()), Times.Never);
    }

    [Fact]
    public async Task Registrar_VeterinarioInexistente_LanzaReglaNegocio()
    {
        ConfigurarBase();
        _veterinarios.Setup(v => v.ObtenerPorIdAsync(2)).ReturnsAsync((Veterinario?)null);

        await Assert.ThrowsAsync<ExcepcionReglaNegocio>(() => _servicio.RegistrarAsync(Solicitud()));
    }

    [Fact]
    public async Task Registrar_TemperaturaFueraDeRango_LanzaReglaNegocio()
    {
        ConfigurarBase();
        var solicitud = Solicitud();
        solicitud.Temperatura = 50m;

        await Assert.ThrowsAsync<ExcepcionReglaNegocio>(() => _servicio.RegistrarAsync(solicitud));
        _consultas.Verify(c => c.CrearAsync(It.IsAny<Consulta>()), Times.Never);
    }

    [Fact]
    public async Task Registrar_CitaDeOtraMascota_LanzaReglaNegocio()
    {
        ConfigurarBase();
        _citas.Setup(c => c.ObtenerPorIdAsync(5)).ReturnsAsync(
            new Cita { Id = 5, MascotaId = 99, VeterinarioId = 2, Estado = EstadosCita.Programada });
        var solicitud = Solicitud();
        solicitud.CitaId = 5;

        await Assert.ThrowsAsync<ExcepcionReglaNegocio>(() => _servicio.RegistrarAsync(solicitud));
    }

    [Fact]
    public async Task Registrar_ConCitaProgramada_MarcaLaCitaComoCompletada()
    {
        ConfigurarBase();
        var cita = new Cita { Id = 5, MascotaId = 1, VeterinarioId = 2, Estado = EstadosCita.Programada };
        _citas.Setup(c => c.ObtenerPorIdAsync(5)).ReturnsAsync(cita);
        var solicitud = Solicitud();
        solicitud.CitaId = 5;

        var respuesta = await _servicio.RegistrarAsync(solicitud);

        Assert.Equal(EstadosCita.Completada, cita.Estado);
        Assert.Equal(5, respuesta.CitaId);
        Assert.Single(respuesta.Diagnosticos);
    }

    [Fact]
    public async Task Registrar_MedicamentoSinExistencias_LanzaReglaNegocio()
    {
        ConfigurarBase();
        _medicamentos.Setup(m => m.ObtenerPorIdAsync(3)).ReturnsAsync(new Medicamento { Id = 3, Nombre = "Amoxicilina", Stock = 0 });
        var solicitud = Solicitud();
        solicitud.Tratamientos.Add(new TratamientoSolicitudDto { MedicamentoId = 3, Indicaciones = "Una tableta cada 12 horas", DuracionDias = 7 });

        await Assert.ThrowsAsync<ExcepcionReglaNegocio>(() => _servicio.RegistrarAsync(solicitud));
        _consultas.Verify(c => c.CrearAsync(It.IsAny<Consulta>()), Times.Never);
    }

    [Fact]
    public async Task Registrar_ConMedicamento_DescuentaUnaUnidadDeExistencias()
    {
        ConfigurarBase();
        var medicamento = new Medicamento { Id = 3, Nombre = "Amoxicilina", Stock = 10 };
        _medicamentos.Setup(m => m.ObtenerPorIdAsync(3)).ReturnsAsync(medicamento);
        var solicitud = Solicitud();
        solicitud.Tratamientos.Add(new TratamientoSolicitudDto { MedicamentoId = 3, Indicaciones = "Una tableta cada 12 horas", DuracionDias = 7 });

        var respuesta = await _servicio.RegistrarAsync(solicitud);

        Assert.Equal(9, medicamento.Stock);
        Assert.Single(respuesta.Tratamientos);
        Assert.Equal(3, respuesta.Tratamientos[0].MedicamentoId);
        Assert.Equal(7, respuesta.Tratamientos[0].DuracionDias);
    }

    [Fact]
    public async Task Registrar_ConPeso_ActualizaElPesoDeLaMascota()
    {
        var mascota = ConfigurarBase();
        var solicitud = Solicitud();
        solicitud.PesoKg = 12.5m;

        var respuesta = await _servicio.RegistrarAsync(solicitud);

        Assert.Equal(12.5m, mascota.PesoKg);
        Assert.Equal(12.5m, respuesta.PesoKg);
    }

    [Fact]
    public async Task Obtener_ConsultaInexistente_LanzaNoEncontrada()
    {
        _consultas.Setup(c => c.ObtenerConDetalleAsync(99)).ReturnsAsync((Consulta?)null);

        await Assert.ThrowsAsync<ExcepcionNoEncontrada>(() => _servicio.ObtenerAsync(99));
    }
}
