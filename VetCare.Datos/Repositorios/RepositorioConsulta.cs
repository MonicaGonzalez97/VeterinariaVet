using Microsoft.EntityFrameworkCore;
using VetCare.Datos.Entidades;

namespace VetCare.Datos.Repositorios;

public interface IRepositorioConsulta : IRepositorio<Consulta>
{
    Task<List<Consulta>> ListarPorMascotaAsync(int? mascotaId);
    Task<Consulta?> ObtenerConDetalleAsync(int id);
}

public class RepositorioConsulta : RepositorioBase<Consulta>, IRepositorioConsulta
{
    public RepositorioConsulta(ContextoVetCare contexto) : base(contexto)
    {
    }

    public async Task<List<Consulta>> ListarPorMascotaAsync(int? mascotaId)
    {
        var consulta = _contexto.Consultas
            .AsNoTracking()
            .Include(c => c.Mascota)
            .Include(c => c.Veterinario)
            .Include(c => c.Diagnosticos)
            .Include(c => c.Tratamientos).ThenInclude(t => t.Medicamento)
            .AsSplitQuery()
            .AsQueryable();

        if (mascotaId.HasValue)
        {
            consulta = consulta.Where(c => c.MascotaId == mascotaId.Value);
        }

        return await consulta.OrderByDescending(c => c.Fecha).ToListAsync();
    }

    public async Task<Consulta?> ObtenerConDetalleAsync(int id)
    {
        return await _contexto.Consultas
            .Include(c => c.Mascota)
            .Include(c => c.Veterinario)
            .Include(c => c.Diagnosticos)
            .Include(c => c.Tratamientos).ThenInclude(t => t.Medicamento)
            .AsSplitQuery()
            .FirstOrDefaultAsync(c => c.Id == id);
    }
}
