namespace VetCare.Datos.Repositorios;

public interface IRepositorio<T> where T : class
{
    Task<T?> ObtenerPorIdAsync(int id);
    Task<List<T>> ListarAsync();
    Task<T> CrearAsync(T entidad);
    Task ActualizarAsync(T entidad);
    Task EliminarAsync(T entidad);
}
