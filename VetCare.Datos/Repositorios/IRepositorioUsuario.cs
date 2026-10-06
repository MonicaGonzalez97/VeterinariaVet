using VetCare.Datos.Entidades;

namespace VetCare.Datos.Repositorios;

public interface IRepositorioUsuario : IRepositorio<Usuario>
{
    Task<Usuario?> BuscarPorCorreoAsync(string correo);
}
