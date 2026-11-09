using Microsoft.EntityFrameworkCore;
using VetCare.Datos.Entidades;

namespace VetCare.Datos.Repositorios;

public interface IRepositorioRegistroVacunacion : IRepositorio<RegistroVacunacion>
{
    Task<List<RegistroVacunacion>> ListarPorMascotaAsync(int? mascotaId);
    Task<RegistroVacunacion?> ObtenerConDetalleAsync(int id);
}

public class RepositorioRegistroVacunacion : RepositorioBase<RegistroVacunacion>, IRepositorioRegistroVacunacion
{
    public RepositorioRegistroVacunacion(ContextoVetCare contexto) : base(contexto)
    {
    }

    public async Task<List<RegistroVacunacion>> ListarPorMascotaAsync(int? mascotaId)
    {
        var consulta = _contexto.RegistrosVacunacion
            .AsNoTracking()
            .Include(r => r.Mascota)
            .Include(r => r.Vacuna)
            .Include(r => r.Veterinario)
            .AsQueryable();

        if (mascotaId.HasValue)
        {
            consulta = consulta.Where(r => r.MascotaId == mascotaId.Value);
        }

        return await consulta.OrderByDescending(r => r.FechaAplicacion).ToListAsync();
    }

    public async Task<RegistroVacunacion?> ObtenerConDetalleAsync(int id)
    {
        return await _contexto.RegistrosVacunacion
            .Include(r => r.Mascota)
            .Include(r => r.Vacuna)
            .Include(r => r.Veterinario)
            .FirstOrDefaultAsync(r => r.Id == id);
    }
}
