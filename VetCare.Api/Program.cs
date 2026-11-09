using System.Reflection;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using VetCare.Api.Inicializacion;
using VetCare.Api.Middleware;
using VetCare.Api.Modelos;
using VetCare.Api.Seguridad;
using VetCare.Datos;
using VetCare.Datos.Entidades;
using VetCare.Datos.Repositorios;
using VetCare.Negocio.Servicios;

var constructor = WebApplication.CreateBuilder(args);

// Los servicios en la nube (Render y otros) indican el puerto en la variable PORT
var puertoNube = Environment.GetEnvironmentVariable("PORT");
if (!string.IsNullOrWhiteSpace(puertoNube))
{
    constructor.WebHost.UseUrls($"http://0.0.0.0:{puertoNube}");
}

// ---------- Configuracion JWT ----------
constructor.Services.Configure<OpcionesJwt>(constructor.Configuration.GetSection(OpcionesJwt.Seccion));
var opcionesJwt = constructor.Configuration.GetSection(OpcionesJwt.Seccion).Get<OpcionesJwt>() ?? new OpcionesJwt();
if (Encoding.UTF8.GetByteCount(opcionesJwt.Clave) < 32)
{
    throw new InvalidOperationException("La clave Jwt:Clave debe tener al menos 32 caracteres");
}

// ---------- Capa de datos ----------
// En la nube la conexion llega en DATABASE_URL; en local se usa appsettings
var cadenaConexion = ConvertirUrlPostgres(Environment.GetEnvironmentVariable("DATABASE_URL"))
    ?? constructor.Configuration.GetConnectionString("VetCare");

constructor.Services.AddDbContext<ContextoVetCare>(opciones =>
    opciones.UseNpgsql(cadenaConexion));

constructor.Services.AddScoped(typeof(IRepositorio<>), typeof(RepositorioBase<>));
constructor.Services.AddScoped<IRepositorioUsuario, RepositorioUsuario>();
constructor.Services.AddScoped<IRepositorioRol, RepositorioRol>();
constructor.Services.AddScoped<IRepositorioCliente, RepositorioCliente>();
constructor.Services.AddScoped<IRepositorioMascota, RepositorioMascota>();
constructor.Services.AddScoped<IRepositorioRaza, RepositorioRaza>();
constructor.Services.AddScoped<IRepositorioVeterinario, RepositorioVeterinario>();
constructor.Services.AddScoped<IRepositorioVacuna, RepositorioVacuna>();
constructor.Services.AddScoped<IRepositorioRegistroVacunacion, RepositorioRegistroVacunacion>();
constructor.Services.AddScoped<IRepositorioCita, RepositorioCita>();
constructor.Services.AddScoped<IRepositorioConsulta, RepositorioConsulta>();
constructor.Services.AddScoped<IRepositorioMedicamento, RepositorioMedicamento>();
constructor.Services.AddScoped<IRepositorioProducto, RepositorioProducto>();
constructor.Services.AddScoped<IRepositorioFactura, RepositorioFactura>();

// ---------- Capa de negocio ----------
constructor.Services.AddScoped<IServicioAutenticacion, ServicioAutenticacion>();
constructor.Services.AddScoped<IServicioCliente, ServicioCliente>();
constructor.Services.AddScoped<IServicioMascota, ServicioMascota>();
constructor.Services.AddScoped<IServicioVeterinario, ServicioVeterinario>();
constructor.Services.AddScoped<IServicioVacunacion, ServicioVacunacion>();
constructor.Services.AddScoped<IServicioCita, ServicioCita>();
constructor.Services.AddScoped<IServicioConsulta, ServicioConsulta>();
constructor.Services.AddScoped<IServicioInventario, ServicioInventario>();
constructor.Services.AddScoped<IServicioFactura, ServicioFactura>();

// ---------- Seguridad ----------
constructor.Services.AddSingleton<IHasheador, HasheadorBcrypt>();
constructor.Services.AddSingleton<IServicioToken, ServicioToken>();

constructor.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(opciones =>
    {
        opciones.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = opcionesJwt.Emisor,
            ValidateAudience = true,
            ValidAudience = opcionesJwt.Audiencia,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(opcionesJwt.Clave)),
            ValidateLifetime = true,
            ClockSkew = TimeSpan.FromMinutes(1),
            NameClaimType = ClaimTypes.Name,
            RoleClaimType = ClaimTypes.Role
        };

        opciones.Events = new JwtBearerEvents
        {
            OnChallenge = async contexto =>
            {
                contexto.HandleResponse();
                contexto.Response.StatusCode = StatusCodes.Status401Unauthorized;
                await contexto.Response.WriteAsJsonAsync(new RespuestaError
                {
                    Estado = 401,
                    Mensaje = "Se requiere un token valido. Inicie sesion en /api/autenticacion/login"
                });
            },
            OnForbidden = async contexto =>
            {
                contexto.Response.StatusCode = StatusCodes.Status403Forbidden;
                await contexto.Response.WriteAsJsonAsync(new RespuestaError
                {
                    Estado = 403,
                    Mensaje = "Su rol no tiene permiso para realizar esta accion"
                });
            }
        };
    });

// Ningun endpoint es accesible sin token, salvo los marcados con [AllowAnonymous]
constructor.Services.AddAuthorization(opciones =>
{
    opciones.FallbackPolicy = new AuthorizationPolicyBuilder().RequireAuthenticatedUser().Build();
});

// ---------- API ----------
constructor.Services
    .AddControllers()
    .ConfigureApiBehaviorOptions(opciones =>
    {
        opciones.InvalidModelStateResponseFactory = contexto =>
        {
            var detalles = contexto.ModelState
                .Where(e => e.Value != null && e.Value.Errors.Count > 0)
                .ToDictionary(
                    e => e.Key,
                    e => e.Value!.Errors.Select(x => string.IsNullOrWhiteSpace(x.ErrorMessage)
                        ? "Valor no valido"
                        : x.ErrorMessage).ToArray());

            return new BadRequestObjectResult(new RespuestaError
            {
                Estado = 400,
                Mensaje = "Los datos enviados no son validos",
                Detalles = detalles
            });
        };
    });

constructor.Services.AddCors(opciones =>
    opciones.AddPolicy("ApiPublica", politica => politica.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod()));

constructor.Services.AddEndpointsApiExplorer();
constructor.Services.AddSwaggerGen(opciones =>
{
    opciones.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "VetCare API",
        Version = "v1",
        Description = "API publica de la veterinaria VetCare. Para probar los endpoints protegidos: " +
                      "inicie sesion en /api/autenticacion/login, copie el token y presione el boton Authorize."
    });

    opciones.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Pegue aqui el token JWT (sin la palabra Bearer)"
    });

    opciones.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
            },
            Array.Empty<string>()
        }
    });

    var rutaXml = Path.Combine(AppContext.BaseDirectory, $"{Assembly.GetExecutingAssembly().GetName().Name}.xml");
    if (File.Exists(rutaXml))
    {
        opciones.IncludeXmlComments(rutaXml);
    }

    // Comentarios de los DTOs (campos de entrada y salida), definidos en VetCare.Negocio
    var rutaXmlNegocio = Path.Combine(AppContext.BaseDirectory, "VetCare.Negocio.xml");
    if (File.Exists(rutaXmlNegocio))
    {
        opciones.IncludeXmlComments(rutaXmlNegocio);
    }
});

var aplicacion = constructor.Build();

// ---------- Crear la base de datos y sembrar datos iniciales ----------
using (var alcance = aplicacion.Services.CreateScope())
{
    var registro = alcance.ServiceProvider.GetRequiredService<ILogger<Program>>();
    try
    {
        var contexto = alcance.ServiceProvider.GetRequiredService<ContextoVetCare>();
        await contexto.Database.EnsureCreatedAsync();

        var hasheador = alcance.ServiceProvider.GetRequiredService<IHasheador>();
        await SembradorDatos.SembrarAsync(contexto, hasheador, aplicacion.Configuration);
    }
    catch (Exception ex)
    {
        registro.LogCritical(ex, "No se pudo preparar la base de datos. Revise la cadena de conexion en appsettings.Development.json");
        throw;
    }
}

// ---------- Pipeline HTTP ----------
aplicacion.UseMiddleware<ManejadorExcepcionesMiddleware>();
aplicacion.UseDefaultFiles();
aplicacion.UseStaticFiles();
aplicacion.UseSwagger();
aplicacion.UseSwaggerUI(opciones =>
{
    opciones.SwaggerEndpoint("/swagger/v1/swagger.json", "VetCare API v1");
    opciones.DocumentTitle = "VetCare API";
});
aplicacion.UseCors("ApiPublica");
aplicacion.UseAuthentication();
aplicacion.UseAuthorization();
aplicacion.MapControllers();

aplicacion.Run();

// Convierte postgres://usuario:clave@servidor:puerto/basedatos al formato que entiende Npgsql
static string? ConvertirUrlPostgres(string? url)
{
    if (string.IsNullOrWhiteSpace(url))
    {
        return null;
    }

    var uri = new Uri(url);
    var credenciales = uri.UserInfo.Split(':', 2);
    var usuario = Uri.UnescapeDataString(credenciales[0]);
    var clave = credenciales.Length > 1 ? Uri.UnescapeDataString(credenciales[1]) : string.Empty;
    var puerto = uri.Port > 0 ? uri.Port : 5432;
    var baseDatos = uri.AbsolutePath.TrimStart('/');

    return $"Host={uri.Host};Port={puerto};Database={baseDatos};Username={usuario};Password={clave};SSL Mode=Prefer;Trust Server Certificate=true";
}
