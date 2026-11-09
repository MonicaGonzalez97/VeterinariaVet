using Microsoft.EntityFrameworkCore;
using VetCare.Datos.Entidades;

namespace VetCare.Datos.Repositorios;

public class RepositorioMascota : RepositorioBase<Mascota>, IRepositorioMascota
{
    public RepositorioMascota(ContextoVetCare contexto) : base(contexto)
    {
    }

    public async Task<List<Mascota>> ListarConDetalleAsync(int? clienteId, bool fallecidas)
    {
        var consulta = _contexto.Mascotas
            .AsNoTracking()
            .Include(m => m.Cliente)
            .Include(m => m.Raza).ThenInclude(r => r!.Especie)
            .Include(m => m.RegistroFallecimiento)
            .Where(m => m.Fallecida == fallecidas);

        if (clienteId.HasValue)
        {
            consulta = consulta.Where(m => m.ClienteId == clienteId.Value);
        }

        return await consulta.OrderBy(m => m.Nombre).ToListAsync();
    }

    public async Task<Mascota?> ObtenerConDetalleAsync(int id)
    {
        return await _contexto.Mascotas
            .Include(m => m.Cliente)
            .Include(m => m.Raza).ThenInclude(r => r!.Especie)
            .Include(m => m.RegistroFallecimiento)
            .FirstOrDefaultAsync(m => m.Id == id);
    }

    public async Task<bool> ExisteNombreParaClienteAsync(int clienteId, string nombre, int? excluirMascotaId)
    {
        var minuscula = nombre.Trim().ToLower();
        return await _contexto.Mascotas.AnyAsync(m =>
            m.ClienteId == clienteId &&
            m.Nombre.ToLower() == minuscula &&
            (!excluirMascotaId.HasValue || m.Id != excluirMascotaId.Value));
    }

    public async Task<bool> TieneHistorialAsync(int mascotaId)
    {
        return await _contexto.Citas.AnyAsync(c => c.MascotaId == mascotaId)
            || await _contexto.Consultas.AnyAsync(c => c.MascotaId == mascotaId)
            || await _contexto.RegistrosVacunacion.AnyAsync(r => r.MascotaId == mascotaId);
    }

    public async Task RegistrarFallecimientoAsync(Mascota mascota, RegistroFallecimiento registro)
    {
        mascota.Fallecida = true;
        registro.MascotaId = mascota.Id;
        _contexto.RegistrosFallecimiento.Add(registro);
        await _contexto.SaveChangesAsync();
        mascota.RegistroFallecimiento = registro;
    }

    public async Task RevertirFallecimientoAsync(Mascota mascota)
    {
        if (mascota.RegistroFallecimiento != null)
        {
            _contexto.RegistrosFallecimiento.Remove(mascota.RegistroFallecimiento);
        }

        mascota.Fallecida = false;
        await _contexto.SaveChangesAsync();
        mascota.RegistroFallecimiento = null;
    }
}
