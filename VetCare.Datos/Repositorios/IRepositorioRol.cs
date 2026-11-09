using VetCare.Datos.Entidades;

namespace VetCare.Datos.Repositorios;

public interface IRepositorioRol : IRepositorio<Rol>
{
    Task<Rol?> BuscarPorNombreAsync(string nombre);
}
