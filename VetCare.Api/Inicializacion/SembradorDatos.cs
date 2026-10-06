using Microsoft.EntityFrameworkCore;
using VetCare.Datos;
using VetCare.Datos.Entidades;
using VetCare.Negocio.Constantes;
using VetCare.Negocio.Servicios;

namespace VetCare.Api.Inicializacion;

public class UsuarioSemilla
{
    public string Nombre { get; set; } = string.Empty;
    public string Correo { get; set; } = string.Empty;
    public string Contraseña { get; set; } = string.Empty;
    public string Rol { get; set; } = string.Empty;
}

/// <summary>Datos iniciales: roles, usuarios de prueba, especies y razas. Solo se ejecuta al arrancar.</summary>
public static class SembradorDatos
{
    public static async Task SembrarAsync(ContextoVetCare contexto, IHasheador hasheador, IConfiguration configuracion)
    {
        await SembrarRolesAsync(contexto);
        await SembrarUsuariosAsync(contexto, hasheador, configuracion);
        await SembrarEspeciesYRazasAsync(contexto);
    }

    private static async Task SembrarRolesAsync(ContextoVetCare contexto)
    {
        if (await contexto.Roles.AnyAsync())
        {
            return;
        }

        contexto.Roles.AddRange(
            new Rol { Nombre = NombresRoles.Administrador, Descripcion = "Control total del sistema" },
            new Rol { Nombre = NombresRoles.Veterinario, Descripcion = "Atiende consultas y registra fallecimientos" },
            new Rol { Nombre = NombresRoles.Recepcionista, Descripcion = "Registra clientes, mascotas y citas" });
        await contexto.SaveChangesAsync();
    }

    private static async Task SembrarUsuariosAsync(ContextoVetCare contexto, IHasheador hasheador, IConfiguration configuracion)
    {
        var usuarios = configuracion.GetSection("Semilla:Usuarios").Get<List<UsuarioSemilla>>() ?? new List<UsuarioSemilla>();

        foreach (var semilla in usuarios)
        {
            var correo = semilla.Correo.Trim().ToLowerInvariant();
            if (await contexto.Usuarios.AnyAsync(u => u.Correo == correo))
            {
                continue;
            }

            var rol = await contexto.Roles.FirstOrDefaultAsync(r => r.Nombre == semilla.Rol);
            if (rol == null)
            {
                continue;
            }

            contexto.Usuarios.Add(new Usuario
            {
                Nombre = semilla.Nombre,
                Correo = correo,
                HashContraseña = hasheador.HashearContraseña(semilla.Contraseña),
                RolId = rol.Id
            });
        }

        await contexto.SaveChangesAsync();
    }

    private static async Task SembrarEspeciesYRazasAsync(ContextoVetCare contexto)
    {
        if (await contexto.Especies.AnyAsync())
        {
            return;
        }

        var catalogo = new Dictionary<string, string[]>
        {
            ["Perro"] = new[] { "Labrador Retriever", "Pastor Aleman", "Chihuahua", "Poodle", "Bulldog", "Criollo (mestizo)" },
            ["Gato"] = new[] { "Siames", "Persa", "Angora", "Criollo (mestizo)" },
            ["Ave"] = new[] { "Canario", "Periquito", "Loro" },
            ["Conejo"] = new[] { "Enano", "Cabeza de Leon" },
            ["Otro"] = new[] { "Otra raza" }
        };

        foreach (var (especie, razas) in catalogo)
        {
            contexto.Especies.Add(new Especie
            {
                Nombre = especie,
                Razas = razas.Select(r => new Raza { Nombre = r }).ToList()
            });
        }

        await contexto.SaveChangesAsync();
    }
}
