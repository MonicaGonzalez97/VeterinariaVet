using Microsoft.EntityFrameworkCore;
using VetCare.Datos.Entidades;

namespace VetCare.Datos.Repositorios;

public class RepositorioUsuario : RepositorioBase<Usuario>, IRepositorioUsuario
{
    public RepositorioUsuario(ContextoVetCare contexto) : base(contexto)
    {
    }

    public async Task<Usuario?> BuscarPorCorreoAsync(string correo)
    {
        return await _contexto.Usuarios
            .Include(u => u.Rol)
            .FirstOrDefaultAsync(u => u.Correo == correo);
    }
}
