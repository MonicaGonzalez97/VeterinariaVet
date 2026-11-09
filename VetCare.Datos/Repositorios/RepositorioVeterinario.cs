using Microsoft.EntityFrameworkCore;
using VetCare.Datos.Entidades;

namespace VetCare.Datos.Repositorios;

public interface IRepositorioVeterinario : IRepositorio<Veterinario>
{
    Task<Veterinario?> BuscarPorColegiadoAsync(string numeroColegiado);
    Task<bool> TieneActividadAsync(int veterinarioId);
}

public class RepositorioVeterinario : RepositorioBase<Veterinario>, IRepositorioVeterinario
{
    public RepositorioVeterinario(ContextoVetCare contexto) : base(contexto)
    {
    }

    public override async Task<List<Veterinario>> ListarAsync()
    {
        return await _contexto.Veterinarios.AsNoTracking()
            .OrderBy(v => v.Apellido).ThenBy(v => v.Nombre).ToListAsync();
    }

    public async Task<Veterinario?> BuscarPorColegiadoAsync(string numeroColegiado)
    {
        return await _contexto.Veterinarios.FirstOrDefaultAsync(v => v.NumeroColegiado == numeroColegiado);
    }

    public async Task<bool> TieneActividadAsync(int veterinarioId)
    {
        return await _contexto.Citas.AnyAsync(c => c.VeterinarioId == veterinarioId)
            || await _contexto.Consultas.AnyAsync(c => c.VeterinarioId == veterinarioId)
            || await _contexto.RegistrosVacunacion.AnyAsync(r => r.VeterinarioId == veterinarioId);
    }
}
