using Microsoft.EntityFrameworkCore;
using VetCare.Datos.Entidades;

namespace VetCare.Datos.Repositorios;

public interface IRepositorioFactura : IRepositorio<Factura>
{
    Task<List<Factura>> ListarConDetalleAsync(int? clienteId, string? estado);
    Task<Factura?> ObtenerConDetalleAsync(int id);
}

public class RepositorioFactura : RepositorioBase<Factura>, IRepositorioFactura
{
    public RepositorioFactura(ContextoVetCare contexto) : base(contexto)
    {
    }

    public async Task<List<Factura>> ListarConDetalleAsync(int? clienteId, string? estado)
    {
        var consulta = _contexto.Facturas
            .AsNoTracking()
            .Include(f => f.Cliente)
            .Include(f => f.Detalles)
            .AsQueryable();

        if (clienteId.HasValue)
        {
            consulta = consulta.Where(f => f.ClienteId == clienteId.Value);
        }

        if (!string.IsNullOrWhiteSpace(estado))
        {
            consulta = consulta.Where(f => f.Estado == estado);
        }

        return await consulta.OrderByDescending(f => f.Fecha).ToListAsync();
    }

    public async Task<Factura?> ObtenerConDetalleAsync(int id)
    {
        return await _contexto.Facturas
            .Include(f => f.Cliente)
            .Include(f => f.Detalles)
            .FirstOrDefaultAsync(f => f.Id == id);
    }
}
