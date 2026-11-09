using Microsoft.EntityFrameworkCore;
using VetCare.Datos.Entidades;

namespace VetCare.Datos.Repositorios;

public interface IRepositorioProducto : IRepositorio<Producto>
{
    Task<List<Producto>> BuscarAsync(string? texto);
    Task<Producto?> BuscarPorNombreAsync(string nombre);
    Task<bool> TieneUsoAsync(int productoId);
}

public class RepositorioProducto : RepositorioBase<Producto>, IRepositorioProducto
{
    public RepositorioProducto(ContextoVetCare contexto) : base(contexto)
    {
    }

    public async Task<List<Producto>> BuscarAsync(string? texto)
    {
        var consulta = _contexto.Productos.AsNoTracking().AsQueryable();

        if (!string.IsNullOrWhiteSpace(texto))
        {
            var termino = texto.Trim().ToLower();
            consulta = consulta.Where(p => p.Nombre.ToLower().Contains(termino));
        }

        return await consulta.OrderBy(p => p.Nombre).ToListAsync();
    }

    public async Task<Producto?> BuscarPorNombreAsync(string nombre)
    {
        var minuscula = nombre.Trim().ToLower();
        return await _contexto.Productos.FirstOrDefaultAsync(p => p.Nombre.ToLower() == minuscula);
    }

    public async Task<bool> TieneUsoAsync(int productoId)
    {
        return await _contexto.DetalleFacturas.AnyAsync(d => d.ProductoId == productoId);
    }
}
