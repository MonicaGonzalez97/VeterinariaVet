using Microsoft.EntityFrameworkCore;
using VetCare.Datos.Entidades;

namespace VetCare.Datos.Repositorios;

public interface IRepositorioCita : IRepositorio<Cita>
{
    Task<List<Cita>> BuscarAsync(DateOnly? fecha, int? veterinarioId, int? mascotaId, string? estado);
    Task<Cita?> ObtenerConDetalleAsync(int id);
    Task<bool> ExisteCruceAsync(int veterinarioId, DateTime fechaHora, int? excluirCitaId);
}

public class RepositorioCita : RepositorioBase<Cita>, IRepositorioCita
{
    public RepositorioCita(ContextoVetCare contexto) : base(contexto)
    {
    }

    public async Task<List<Cita>> BuscarAsync(DateOnly? fecha, int? veterinarioId, int? mascotaId, string? estado)
    {
        var consulta = _contexto.Citas
            .AsNoTracking()
            .Include(c => c.Mascota).ThenInclude(m => m!.Cliente)
            .Include(c => c.Veterinario)
            .AsQueryable();

        if (fecha.HasValue)
        {
            var inicio = DateTime.SpecifyKind(fecha.Value.ToDateTime(TimeOnly.MinValue), DateTimeKind.Utc);
            var fin = inicio.AddDays(1);
            consulta = consulta.Where(c => c.FechaHora >= inicio && c.FechaHora < fin);
        }

        if (veterinarioId.HasValue)
        {
            consulta = consulta.Where(c => c.VeterinarioId == veterinarioId.Value);
        }

        if (mascotaId.HasValue)
        {
            consulta = consulta.Where(c => c.MascotaId == mascotaId.Value);
        }

        if (!string.IsNullOrWhiteSpace(estado))
        {
            consulta = consulta.Where(c => c.Estado == estado);
        }

        return await consulta.OrderBy(c => c.FechaHora).ToListAsync();
    }

    public async Task<Cita?> ObtenerConDetalleAsync(int id)
    {
        return await _contexto.Citas
            .Include(c => c.Mascota).ThenInclude(m => m!.Cliente)
            .Include(c => c.Veterinario)
            .FirstOrDefaultAsync(c => c.Id == id);
    }

    // Hay cruce si el veterinario tiene otra cita no cancelada a menos de 30 minutos
    public async Task<bool> ExisteCruceAsync(int veterinarioId, DateTime fechaHora, int? excluirCitaId)
    {
        var desde = fechaHora.AddMinutes(-30);
        var hasta = fechaHora.AddMinutes(30);

        return await _contexto.Citas.AnyAsync(c =>
            c.VeterinarioId == veterinarioId &&
            c.Estado != "Cancelada" &&
            c.FechaHora > desde &&
            c.FechaHora < hasta &&
            (!excluirCitaId.HasValue || c.Id != excluirCitaId.Value));
    }
}
