using Microsoft.EntityFrameworkCore;
using VetCare.Datos.Entidades;

namespace VetCare.Datos.Repositorios;

public interface IRepositorioVacuna : IRepositorio<Vacuna>
{
    Task<Vacuna?> BuscarPorNombreAsync(string nombre);
    Task<bool> TieneRegistrosAsync(int vacunaId);
}

public class RepositorioVacuna : RepositorioBase<Vacuna>, IRepositorioVacuna
{
    public RepositorioVacuna(ContextoVetCare contexto) : base(contexto)
    {
    }

    public override async Task<List<Vacuna>> ListarAsync()
    {
        return await _contexto.Vacunas.AsNoTracking().OrderBy(v => v.Nombre).ToListAsync();
    }

    public async Task<Vacuna?> BuscarPorNombreAsync(string nombre)
    {
        var minuscula = nombre.Trim().ToLower();
        return await _contexto.Vacunas.FirstOrDefaultAsync(v => v.Nombre.ToLower() == minuscula);
    }

    public async Task<bool> TieneRegistrosAsync(int vacunaId)
    {
        return await _contexto.RegistrosVacunacion.AnyAsync(r => r.VacunaId == vacunaId);
    }
}
