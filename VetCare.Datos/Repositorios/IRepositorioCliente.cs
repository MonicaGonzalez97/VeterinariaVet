using VetCare.Datos.Entidades;

namespace VetCare.Datos.Repositorios;

public interface IRepositorioCliente : IRepositorio<Cliente>
{
    Task<List<Cliente>> BuscarAsync(string? texto);
    Task<Cliente?> BuscarPorDocumentoAsync(string documento);
    Task<bool> TieneMascotasAsync(int clienteId);
}
