using Microsoft.EntityFrameworkCore;
using VetCare.Datos.Entidades;

namespace VetCare.Datos.Repositorios;

public class RepositorioRol : RepositorioBase<Rol>, IRepositorioRol
{
    public RepositorioRol(ContextoVetCare contexto) : base(contexto)
    {
    }

    public async Task<Rol?> BuscarPorNombreAsync(string nombre)
    {
        return await _contexto.Roles.FirstOrDefaultAsync(r => r.Nombre == nombre);
    }
}
