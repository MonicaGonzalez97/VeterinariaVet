using Microsoft.EntityFrameworkCore;
using VetCare.Datos.Entidades;

namespace VetCare.Datos.Repositorios;

public interface IRepositorioMedicamento : IRepositorio<Medicamento>
{
    Task<List<Medicamento>> BuscarAsync(string? texto);
    Task<Medicamento?> BuscarPorNombreAsync(string nombre);
    Task<bool> TieneUsoAsync(int medicamentoId);
}

public class RepositorioMedicamento : RepositorioBase<Medicamento>, IRepositorioMedicamento
{
    public RepositorioMedicamento(ContextoVetCare contexto) : base(contexto)
    {
    }

    public async Task<List<Medicamento>> BuscarAsync(string? texto)
    {
        var consulta = _contexto.Medicamentos.AsNoTracking().AsQueryable();

        if (!string.IsNullOrWhiteSpace(texto))
        {
            var termino = texto.Trim().ToLower();
            consulta = consulta.Where(m => m.Nombre.ToLower().Contains(termino));
        }

        return await consulta.OrderBy(m => m.Nombre).ToListAsync();
    }

    public async Task<Medicamento?> BuscarPorNombreAsync(string nombre)
    {
        var minuscula = nombre.Trim().ToLower();
        return await _contexto.Medicamentos.FirstOrDefaultAsync(m => m.Nombre.ToLower() == minuscula);
    }

    public async Task<bool> TieneUsoAsync(int medicamentoId)
    {
        return await _contexto.Tratamientos.AnyAsync(t => t.MedicamentoId == medicamentoId);
    }
}
