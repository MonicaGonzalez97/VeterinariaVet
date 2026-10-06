using Microsoft.EntityFrameworkCore;

namespace VetCare.Datos.Repositorios;

public class RepositorioBase<T> : IRepositorio<T> where T : class
{
    protected readonly ContextoVetCare _contexto;

    public RepositorioBase(ContextoVetCare contexto)
    {
        _contexto = contexto;
    }

    public virtual async Task<T?> ObtenerPorIdAsync(int id)
    {
        return await _contexto.Set<T>().FindAsync(id);
    }

    public virtual async Task<List<T>> ListarAsync()
    {
        return await _contexto.Set<T>().AsNoTracking().ToListAsync();
    }

    public virtual async Task<T> CrearAsync(T entidad)
    {
        _contexto.Set<T>().Add(entidad);
        await _contexto.SaveChangesAsync();
        return entidad;
    }

    public virtual async Task ActualizarAsync(T entidad)
    {
        _contexto.Set<T>().Update(entidad);
        await _contexto.SaveChangesAsync();
    }

    public virtual async Task EliminarAsync(T entidad)
    {
        _contexto.Set<T>().Remove(entidad);
        await _contexto.SaveChangesAsync();
    }
}
