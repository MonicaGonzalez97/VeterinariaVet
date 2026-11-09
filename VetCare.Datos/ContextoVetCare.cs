using System.Text.RegularExpressions;
using Microsoft.EntityFrameworkCore;
using VetCare.Datos.Entidades;

namespace VetCare.Datos;

public class ContextoVetCare : DbContext
{
    public ContextoVetCare(DbContextOptions<ContextoVetCare> opciones) : base(opciones)
    {
    }

    public DbSet<Rol> Roles => Set<Rol>();
    public DbSet<Usuario> Usuarios => Set<Usuario>();
    public DbSet<Cliente> Clientes => Set<Cliente>();
    public DbSet<Especie> Especies => Set<Especie>();
    public DbSet<Raza> Razas => Set<Raza>();
    public DbSet<Mascota> Mascotas => Set<Mascota>();
    public DbSet<Veterinario> Veterinarios => Set<Veterinario>();
    public DbSet<Cita> Citas => Set<Cita>();
    public DbSet<Consulta> Consultas => Set<Consulta>();
    public DbSet<Diagnostico> Diagnosticos => Set<Diagnostico>();
    public DbSet<Medicamento> Medicamentos => Set<Medicamento>();
    public DbSet<Tratamiento> Tratamientos => Set<Tratamiento>();
    public DbSet<RegistroFallecimiento> RegistrosFallecimiento => Set<RegistroFallecimiento>();
    public DbSet<Vacuna> Vacunas => Set<Vacuna>();
    public DbSet<RegistroVacunacion> RegistrosVacunacion => Set<RegistroVacunacion>();
    public DbSet<Producto> Productos => Set<Producto>();
    public DbSet<Factura> Facturas => Set<Factura>();
    public DbSet<DetalleFactura> DetalleFacturas => Set<DetalleFactura>();

    protected override void OnModelCreating(ModelBuilder constructor)
    {
        base.OnModelCreating(constructor);

        constructor.Entity<Rol>().HasIndex(r => r.Nombre).IsUnique();
        constructor.Entity<Usuario>().HasIndex(u => u.Correo).IsUnique();
        constructor.Entity<Cliente>().HasIndex(c => c.Documento).IsUnique();
        constructor.Entity<Especie>().HasIndex(e => e.Nombre).IsUnique();
        constructor.Entity<Veterinario>().HasIndex(v => v.NumeroColegiado).IsUnique();

        constructor.Entity<Mascota>().Property(m => m.PesoKg).HasPrecision(6, 2);
        constructor.Entity<Consulta>().Property(c => c.PesoKg).HasPrecision(6, 2);
        constructor.Entity<Consulta>().Property(c => c.Temperatura).HasPrecision(4, 1);
        constructor.Entity<Producto>().Property(p => p.Precio).HasPrecision(10, 2);
        constructor.Entity<Factura>().Property(f => f.Total).HasPrecision(12, 2);
        constructor.Entity<DetalleFactura>().Property(d => d.PrecioUnitario).HasPrecision(10, 2);

        // Una mascota tiene como maximo un registro de fallecimiento (uno a uno)
        constructor.Entity<RegistroFallecimiento>()
            .HasOne(r => r.Mascota)
            .WithOne(m => m.RegistroFallecimiento)
            .HasForeignKey<RegistroFallecimiento>(r => r.MascotaId);
        constructor.Entity<RegistroFallecimiento>().HasIndex(r => r.MascotaId).IsUnique();

        // Una consulta puede venir de una cita (relacion uno a uno opcional)
        constructor.Entity<Consulta>()
            .HasOne(c => c.Cita)
            .WithOne(c => c.Consulta)
            .HasForeignKey<Consulta>(c => c.CitaId);

        // Nunca se borra en cascada: el negocio decide que se puede eliminar
        foreach (var llave in constructor.Model.GetEntityTypes().SelectMany(e => e.GetForeignKeys()))
        {
            llave.DeleteBehavior = DeleteBehavior.Restrict;
        }

        // Nombres de tablas y columnas en minuscula con guiones bajos (clientes, fecha_nacimiento)
        foreach (var entidad in constructor.Model.GetEntityTypes())
        {
            var nombreTabla = entidad.GetTableName();
            if (nombreTabla != null)
            {
                entidad.SetTableName(AMinusculaConGuiones(nombreTabla));
            }

            foreach (var propiedad in entidad.GetProperties())
            {
                propiedad.SetColumnName(AMinusculaConGuiones(propiedad.Name));
            }
        }
    }

    private static string AMinusculaConGuiones(string texto)
    {
        return Regex.Replace(texto, "(?<!^)([A-Z])", "_$1").ToLowerInvariant();
    }
}
