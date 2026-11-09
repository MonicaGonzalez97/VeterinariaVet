using VetCare.Datos.Entidades;

namespace VetCare.Datos.Repositorios;

public interface IRepositorioMascota : IRepositorio<Mascota>
{
    Task<List<Mascota>> ListarConDetalleAsync(int? clienteId, bool fallecidas);
    Task<Mascota?> ObtenerConDetalleAsync(int id);
    Task<bool> ExisteNombreParaClienteAsync(int clienteId, string nombre, int? excluirMascotaId);
    Task<bool> TieneHistorialAsync(int mascotaId);
    Task RegistrarFallecimientoAsync(Mascota mascota, RegistroFallecimiento registro);
    Task RevertirFallecimientoAsync(Mascota mascota);
}
