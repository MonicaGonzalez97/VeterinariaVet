using Microsoft.EntityFrameworkCore;
using VetCare.Datos.Entidades;

namespace VetCare.Datos.Repositorios;

public class RepositorioRaza : RepositorioBase<Raza>, IRepositorioRaza
{
    public RepositorioRaza(ContextoVetCare contexto) : base(contexto)
    {
    }

    public async Task<List<Raza>> ListarPorEspecieAsync(int? especieId)
    {
        var consulta = _contexto.Razas.AsNoTracking().Include(r => r.Especie).AsQueryable();

        if (especieId.HasValue)
        {
            consulta = consulta.Where(r => r.EspecieId == especieId.Value);
        }

        return await consulta.OrderBy(r => r.Nombre).ToListAsync();
    }
}
