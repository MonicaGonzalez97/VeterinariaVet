using Microsoft.EntityFrameworkCore;
using VetCare.Datos.Entidades;

namespace VetCare.Datos.Repositorios;

public class RepositorioCliente : RepositorioBase<Cliente>, IRepositorioCliente
{
    public RepositorioCliente(ContextoVetCare contexto) : base(contexto)
    {
    }

    public async Task<List<Cliente>> BuscarAsync(string? texto)
    {
        var consulta = _contexto.Clientes.AsNoTracking().AsQueryable();

        if (!string.IsNullOrWhiteSpace(texto))
        {
            var termino = texto.Trim().ToLower();
            consulta = consulta.Where(c =>
                c.Nombre.ToLower().Contains(termino) ||
                c.Apellido.ToLower().Contains(termino) ||
                c.Documento.Contains(termino));
        }

        return await consulta.OrderBy(c => c.Apellido).ThenBy(c => c.Nombre).ToListAsync();
    }

    public async Task<Cliente?> BuscarPorDocumentoAsync(string documento)
    {
        return await _contexto.Clientes.FirstOrDefaultAsync(c => c.Documento == documento);
    }

    public async Task<bool> TieneMascotasAsync(int clienteId)
    {
        return await _contexto.Mascotas.AnyAsync(m => m.ClienteId == clienteId);
    }
}
