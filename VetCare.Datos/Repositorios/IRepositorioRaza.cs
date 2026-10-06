using VetCare.Datos.Entidades;

namespace VetCare.Datos.Repositorios;

public interface IRepositorioRaza : IRepositorio<Raza>
{
    Task<List<Raza>> ListarPorEspecieAsync(int? especieId);
}
