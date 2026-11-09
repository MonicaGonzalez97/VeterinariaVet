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

/// <summary>Datos iniciales: roles, usuarios de prueba, especies, razas y (opcional) datos de ejemplo. Solo se ejecuta al arrancar.</summary>
public static class SembradorDatos
{
    public static async Task SembrarAsync(ContextoVetCare contexto, IHasheador hasheador, IConfiguration configuracion)
    {
        await SembrarRolesAsync(contexto);
        await SembrarUsuariosAsync(contexto, hasheador, configuracion);
        await SembrarEspeciesYRazasAsync(contexto);
        await SembrarCatalogosClinicaAsync(contexto);
        await SembrarDatosDemoAsync(contexto, configuracion);
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

    // En la nube las contraseñas de las cuentas de prueba llegan por variables de entorno
    private static string? ClaveDeEntorno(string rol)
    {
        return rol switch
        {
            NombresRoles.Administrador => Environment.GetEnvironmentVariable("SEMILLA_CLAVE_ADMIN"),
            NombresRoles.Veterinario => Environment.GetEnvironmentVariable("SEMILLA_CLAVE_VETERINARIO"),
            NombresRoles.Recepcionista => Environment.GetEnvironmentVariable("SEMILLA_CLAVE_RECEPCION"),
            _ => null
        };
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

            var claveEntorno = ClaveDeEntorno(semilla.Rol);
            var clave = string.IsNullOrWhiteSpace(claveEntorno) ? semilla.Contraseña : claveEntorno;

            contexto.Usuarios.Add(new Usuario
            {
                Nombre = semilla.Nombre,
                Correo = correo,
                HashContraseña = hasheador.HashearContraseña(clave),
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


    /// <summary>Catalogos iniciales de la clinica: vacunas, veterinarios, medicamentos y productos. Cada uno se carga solo si su tabla esta vacia.</summary>
    private static async Task SembrarCatalogosClinicaAsync(ContextoVetCare contexto)
    {
        if (!await contexto.Vacunas.AnyAsync())
        {
            contexto.Vacunas.AddRange(
                new Vacuna { Nombre = "Rabia", IntervaloDias = 365 },
                new Vacuna { Nombre = "Parvovirus", IntervaloDias = 365 },
                new Vacuna { Nombre = "Moquillo", IntervaloDias = 365 },
                new Vacuna { Nombre = "Bordetella (tos de las perreras)", IntervaloDias = 180 },
                new Vacuna { Nombre = "Triple felina", IntervaloDias = 365 },
                new Vacuna { Nombre = "Desparasitacion interna", IntervaloDias = 90 });
        }

        if (!await contexto.Veterinarios.AnyAsync())
        {
            contexto.Veterinarios.AddRange(
                new Veterinario { Nombre = "Laura", Apellido = "Ramírez", NumeroColegiado = "VET-1001", Especialidad = "Medicina interna" },
                new Veterinario { Nombre = "Andrés", Apellido = "Castillo", NumeroColegiado = "VET-1002", Especialidad = "Cirugía y traumatología" });
        }

        if (!await contexto.Medicamentos.AnyAsync())
        {
            contexto.Medicamentos.AddRange(
                new Medicamento { Nombre = "Amoxicilina", Presentacion = "Tabletas 250 mg", Stock = 60 },
                new Medicamento { Nombre = "Meloxicam", Presentacion = "Suspension 1.5 mg/ml", Stock = 25 },
                new Medicamento { Nombre = "Ivermectina", Presentacion = "Inyectable 1%", Stock = 15 },
                new Medicamento { Nombre = "Prednisolona", Presentacion = "Tabletas 5 mg", Stock = 40 },
                new Medicamento { Nombre = "Omeprazol", Presentacion = "Capsulas 20 mg", Stock = 30 },
                new Medicamento { Nombre = "Gotas oticas", Presentacion = "Frasco 15 ml", Stock = 12 });
        }

        if (!await contexto.Productos.AnyAsync())
        {
            contexto.Productos.AddRange(
                new Producto { Nombre = "Concentrado para perro adulto 15 lb", Categoria = "Alimento", Precio = 185.00m, Stock = 20 },
                new Producto { Nombre = "Concentrado para gato adulto 8 lb", Categoria = "Alimento", Precio = 140.00m, Stock = 18 },
                new Producto { Nombre = "Shampoo antipulgas", Categoria = "Higiene", Precio = 55.00m, Stock = 25 },
                new Producto { Nombre = "Collar antipulgas", Categoria = "Antiparasitarios", Precio = 95.00m, Stock = 14 },
                new Producto { Nombre = "Pipeta antiparasitaria externa", Categoria = "Antiparasitarios", Precio = 80.00m, Stock = 22 },
                new Producto { Nombre = "Juguete mordedera", Categoria = "Accesorios", Precio = 35.00m, Stock = 30 },
                new Producto { Nombre = "Arena sanitaria 10 lb", Categoria = "Higiene", Precio = 60.00m, Stock = 16 },
                new Producto { Nombre = "Correa ajustable", Categoria = "Accesorios", Precio = 45.00m, Stock = 12 });
        }

        await contexto.SaveChangesAsync();
    }

    /// <summary>
    /// Clientes y mascotas ficticios para la demostracion. Solo se cargan si Semilla:CargarDatosDemo es verdadero
    /// y la tabla de clientes esta vacia, por lo que nunca duplica ni pisa datos reales.
    /// </summary>
    private static async Task SembrarDatosDemoAsync(ContextoVetCare contexto, IConfiguration configuracion)
    {
        if (!configuracion.GetValue<bool>("Semilla:CargarDatosDemo") || await contexto.Clientes.AnyAsync())
        {
            return;
        }

        var clientes = new List<Cliente>
        {
            new() { Nombre = "Ana Lucía", Apellido = "Morales", Documento = "2001000000101", Telefono = "55501001", Correo = "ana.morales@ejemplo.com", Direccion = "Zona 3, Quetzaltenango" },
            new() { Nombre = "Carlos", Apellido = "Méndez", Documento = "2001000000202", Telefono = "55501002", Correo = "carlos.mendez@ejemplo.com", Direccion = "Zona 1, Quetzaltenango" },
            new() { Nombre = "Sofía", Apellido = "Hernández", Documento = "2001000000303", Telefono = "55501003", Correo = "sofia.hernandez@ejemplo.com", Direccion = "Zona 5, Quetzaltenango" },
            new() { Nombre = "Luis", Apellido = "Pérez", Documento = "2001000000404", Telefono = "55501004", Correo = null, Direccion = "Cantel, Quetzaltenango" },
            new() { Nombre = "Marta", Apellido = "Gómez", Documento = "2001000000505", Telefono = "55501005", Correo = "marta.gomez@ejemplo.com", Direccion = "Zona 2, Quetzaltenango" },
            new() { Nombre = "Jorge", Apellido = "Tzoc", Documento = "2001000000606", Telefono = "55501006", Correo = null, Direccion = "Salcajá, Quetzaltenango" },
            new() { Nombre = "Daniela", Apellido = "López", Documento = "2001000000707", Telefono = "55501007", Correo = "daniela.lopez@ejemplo.com", Direccion = "Zona 7, Quetzaltenango" },
            new() { Nombre = "Pedro", Apellido = "Xicay", Documento = "2001000000808", Telefono = "55501008", Correo = "pedro.xicay@ejemplo.com", Direccion = "Almolonga, Quetzaltenango" }
        };

        contexto.Clientes.AddRange(clientes);
        await contexto.SaveChangesAsync();

        // Clave: "Especie|Raza", porque algunas razas se llaman igual en distintas especies
        var razas = await contexto.Razas
            .Include(r => r.Especie)
            .ToDictionaryAsync(r => r.Especie!.Nombre + "|" + r.Nombre, r => r.Id);

        var mascotas = new List<Mascota>();

        void Agregar(int indiceCliente, string nombre, string razaClave, DateOnly nacimiento, string sexo,
            decimal pesoKg, string color, DateOnly? fallecio = null, string? causa = null, string? observaciones = null)
        {
            var mascota = new Mascota
            {
                Nombre = nombre,
                ClienteId = clientes[indiceCliente].Id,
                RazaId = razas[razaClave],
                FechaNacimiento = nacimiento,
                Sexo = sexo,
                PesoKg = pesoKg,
                Color = color
            };

            if (fallecio.HasValue)
            {
                mascota.Fallecida = true;
                mascota.RegistroFallecimiento = new RegistroFallecimiento
                {
                    FechaFallecimiento = fallecio.Value,
                    Causa = causa,
                    Observaciones = observaciones
                };
            }

            mascotas.Add(mascota);
        }

        Agregar(0, "Firulais", "Perro|Labrador Retriever", new DateOnly(2019, 3, 12), "Macho", 28.40m, "Dorado");
        Agregar(0, "Luna", "Gato|Siames", new DateOnly(2021, 7, 2), "Hembra", 3.80m, "Crema");
        Agregar(1, "Max", "Perro|Pastor Aleman", new DateOnly(2017, 11, 20), "Macho", 34.00m, "Negro con fuego");
        Agregar(2, "Bella", "Perro|Poodle", new DateOnly(2020, 1, 15), "Hembra", 6.20m, "Blanco");
        Agregar(3, "Michi", "Gato|Criollo (mestizo)", new DateOnly(2022, 5, 30), "Macho", 4.10m, "Gris");
        Agregar(4, "Rocky", "Perro|Bulldog", new DateOnly(2018, 9, 9), "Macho", 24.50m, "Atigrado");
        Agregar(5, "Pelusa", "Conejo|Enano", new DateOnly(2023, 2, 14), "Hembra", 1.30m, "Blanco");
        Agregar(6, "Kiwi", "Ave|Periquito", new DateOnly(2024, 4, 1), "Macho", 0.04m, "Verde");
        Agregar(6, "Canela", "Perro|Chihuahua", new DateOnly(2021, 12, 25), "Hembra", 2.60m, "Café");
        Agregar(7, "Simba", "Gato|Persa", new DateOnly(2019, 8, 18), "Macho", 5.30m, "Naranja");

        // Mascotas fallecidas: aparecen en el Memorial
        Agregar(1, "Toby", "Perro|Criollo (mestizo)", new DateOnly(2008, 6, 1), "Macho", 18.00m, "Café",
            new DateOnly(2025, 9, 14), "Insuficiencia renal", "Estuvo acompañado por su familia hasta el final.");
        Agregar(4, "Nala", "Gato|Angora", new DateOnly(2010, 3, 3), "Hembra", 3.50m, "Blanco",
            new DateOnly(2026, 2, 20), "Vejez", null);

        contexto.Mascotas.AddRange(mascotas);
        await contexto.SaveChangesAsync();

        // Citas programadas, una consulta atendida y una factura de ejemplo
        var veterinarios = await contexto.Veterinarios.OrderBy(v => v.Id).ToListAsync();
        if (veterinarios.Count >= 2)
        {
            var manana = DateTime.UtcNow.Date.AddDays(1);
            contexto.Citas.AddRange(
                new Cita { MascotaId = mascotas[0].Id, VeterinarioId = veterinarios[0].Id, FechaHora = manana.AddHours(9), Motivo = "Vacunacion anual", Estado = "Programada" },
                new Cita { MascotaId = mascotas[1].Id, VeterinarioId = veterinarios[0].Id, FechaHora = manana.AddHours(10), Motivo = "Control de peso", Estado = "Programada" },
                new Cita { MascotaId = mascotas[2].Id, VeterinarioId = veterinarios[1].Id, FechaHora = manana.AddHours(11), Motivo = "Revision de cadera", Estado = "Programada" },
                new Cita { MascotaId = mascotas[3].Id, VeterinarioId = veterinarios[1].Id, FechaHora = manana.AddDays(1).AddHours(15), Motivo = "Limpieza dental", Estado = "Programada" });

            contexto.Consultas.Add(new Consulta
            {
                Fecha = DateTime.UtcNow.AddDays(-3),
                MascotaId = mascotas[4].Id,
                VeterinarioId = veterinarios[0].Id,
                PesoKg = 4.10m,
                Temperatura = 38.6m,
                Observaciones = "Rascado frecuente de orejas desde hace una semana.",
                Diagnosticos = new List<Diagnostico> { new() { Descripcion = "Otitis externa", Gravedad = "Leve" } },
                Tratamientos = new List<Tratamiento> { new() { Indicaciones = "Limpiar y aplicar 3 gotas en cada oido cada 12 horas", Dosis = "3 gotas", DuracionDias = 7 } }
            });
        }

        var concentrado = await contexto.Productos.FirstOrDefaultAsync(p => p.Nombre.StartsWith("Concentrado para perro"));
        if (concentrado != null)
        {
            concentrado.Stock -= 1;
            contexto.Facturas.Add(new Factura
            {
                ClienteId = clientes[0].Id,
                Fecha = DateTime.UtcNow.AddDays(-1),
                Estado = "Pagada",
                Total = concentrado.Precio + 150.00m,
                Detalles = new List<DetalleFactura>
                {
                    new() { ProductoId = concentrado.Id, Descripcion = concentrado.Nombre, Cantidad = 1, PrecioUnitario = concentrado.Precio },
                    new() { Descripcion = "Consulta veterinaria", Cantidad = 1, PrecioUnitario = 150.00m }
                }
            });
        }

        await contexto.SaveChangesAsync();
    }
}
