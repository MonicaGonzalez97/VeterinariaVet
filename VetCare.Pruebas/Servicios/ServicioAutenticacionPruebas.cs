using Moq;
using VetCare.Datos.Entidades;
using VetCare.Datos.Repositorios;
using VetCare.Negocio.Constantes;
using VetCare.Negocio.Dtos;
using VetCare.Negocio.Excepciones;
using VetCare.Negocio.Servicios;

namespace VetCare.Pruebas.Servicios;

public class ServicioAutenticacionPruebas
{
    private readonly Mock<IRepositorioUsuario> _usuarios = new();
    private readonly Mock<IRepositorioRol> _roles = new();
    private readonly Mock<IHasheador> _hasheador = new();
    private readonly Mock<IServicioToken> _servicioToken = new();
    private readonly ServicioAutenticacion _servicio;

    public ServicioAutenticacionPruebas()
    {
        _servicioToken
            .Setup(t => t.GenerarToken(It.IsAny<Usuario>()))
            .Returns((Usuario u) => new RespuestaAutenticacionDto
            {
                Token = "token-de-prueba",
                Correo = u.Correo,
                Nombre = u.Nombre,
                Rol = u.Rol?.Nombre ?? string.Empty
            });

        _servicio = new ServicioAutenticacion(_usuarios.Object, _roles.Object, _hasheador.Object, _servicioToken.Object);
    }

    private static Usuario UsuarioActivo(bool activo = true) => new()
    {
        Id = 1,
        Nombre = "Ana Lopez",
        Correo = "ana@vetcare.com",
        HashContraseña = "hash-guardado",
        Activo = activo,
        Rol = new Rol { Id = 1, Nombre = NombresRoles.Administrador }
    };

    [Fact]
    public async Task Registrar_CorreoDuplicado_LanzaConflicto()
    {
        _usuarios.Setup(u => u.BuscarPorCorreoAsync("ana@vetcare.com")).ReturnsAsync(UsuarioActivo());

        var dto = new RegistroDto { Nombre = "Ana", Correo = "ana@vetcare.com", Contraseña = "Clave12345" };

        await Assert.ThrowsAsync<ExcepcionConflicto>(() => _servicio.RegistrarAsync(dto));
        _usuarios.Verify(u => u.CrearAsync(It.IsAny<Usuario>()), Times.Never);
    }

    [Fact]
    public async Task Registrar_DatosValidos_CreaRecepcionistaConContraseñaHasheada()
    {
        _usuarios.Setup(u => u.BuscarPorCorreoAsync("nuevo@vetcare.com")).ReturnsAsync((Usuario?)null);
        _roles.Setup(r => r.BuscarPorNombreAsync(NombresRoles.Recepcionista))
            .ReturnsAsync(new Rol { Id = 3, Nombre = NombresRoles.Recepcionista });
        _hasheador.Setup(h => h.HashearContraseña("Clave12345")).Returns("hash-seguro");

        Usuario? creado = null;
        _usuarios.Setup(u => u.CrearAsync(It.IsAny<Usuario>()))
            .Callback<Usuario>(u => creado = u)
            .ReturnsAsync((Usuario u) => u);

        var respuesta = await _servicio.RegistrarAsync(
            new RegistroDto { Nombre = "  Nuevo Usuario ", Correo = "NUEVO@vetcare.com", Contraseña = "Clave12345" });

        Assert.NotNull(creado);
        Assert.Equal("nuevo@vetcare.com", creado!.Correo);
        Assert.Equal("Nuevo Usuario", creado.Nombre);
        Assert.Equal("hash-seguro", creado.HashContraseña);
        Assert.NotEqual("Clave12345", creado.HashContraseña);
        Assert.Equal(3, creado.RolId);
        Assert.Equal("token-de-prueba", respuesta.Token);
        Assert.Equal(NombresRoles.Recepcionista, respuesta.Rol);
    }

    [Fact]
    public async Task Registrar_RolPorDefectoNoExiste_LanzaReglaNegocio()
    {
        _usuarios.Setup(u => u.BuscarPorCorreoAsync(It.IsAny<string>())).ReturnsAsync((Usuario?)null);
        _roles.Setup(r => r.BuscarPorNombreAsync(It.IsAny<string>())).ReturnsAsync((Rol?)null);

        var dto = new RegistroDto { Nombre = "Ana", Correo = "ana@vetcare.com", Contraseña = "Clave12345" };

        await Assert.ThrowsAsync<ExcepcionReglaNegocio>(() => _servicio.RegistrarAsync(dto));
        _usuarios.Verify(u => u.CrearAsync(It.IsAny<Usuario>()), Times.Never);
    }

    [Fact]
    public async Task IniciarSesion_CorreoInexistente_LanzaNoAutenticado()
    {
        _usuarios.Setup(u => u.BuscarPorCorreoAsync(It.IsAny<string>())).ReturnsAsync((Usuario?)null);

        await Assert.ThrowsAsync<ExcepcionNoAutenticado>(() =>
            _servicio.IniciarSesionAsync(new LoginDto { Correo = "nadie@vetcare.com", Contraseña = "Clave12345" }));
    }

    [Fact]
    public async Task IniciarSesion_ContraseñaIncorrecta_LanzaNoAutenticado()
    {
        _usuarios.Setup(u => u.BuscarPorCorreoAsync("ana@vetcare.com")).ReturnsAsync(UsuarioActivo());
        _hasheador.Setup(h => h.VerificarContraseña("equivocada", "hash-guardado")).Returns(false);

        await Assert.ThrowsAsync<ExcepcionNoAutenticado>(() =>
            _servicio.IniciarSesionAsync(new LoginDto { Correo = "ana@vetcare.com", Contraseña = "equivocada" }));
        _servicioToken.Verify(t => t.GenerarToken(It.IsAny<Usuario>()), Times.Never);
    }

    [Fact]
    public async Task IniciarSesion_UsuarioInactivo_LanzaNoAutenticadoAunqueLaContraseñaSeaCorrecta()
    {
        _usuarios.Setup(u => u.BuscarPorCorreoAsync("ana@vetcare.com")).ReturnsAsync(UsuarioActivo(activo: false));
        _hasheador.Setup(h => h.VerificarContraseña(It.IsAny<string>(), It.IsAny<string>())).Returns(true);

        await Assert.ThrowsAsync<ExcepcionNoAutenticado>(() =>
            _servicio.IniciarSesionAsync(new LoginDto { Correo = "ana@vetcare.com", Contraseña = "Clave12345" }));
        _servicioToken.Verify(t => t.GenerarToken(It.IsAny<Usuario>()), Times.Never);
    }

    [Fact]
    public async Task IniciarSesion_CredencialesValidas_DevuelveTokenConElRolDelUsuario()
    {
        _usuarios.Setup(u => u.BuscarPorCorreoAsync("ana@vetcare.com")).ReturnsAsync(UsuarioActivo());
        _hasheador.Setup(h => h.VerificarContraseña("Clave12345", "hash-guardado")).Returns(true);

        var respuesta = await _servicio.IniciarSesionAsync(
            new LoginDto { Correo = " ANA@vetcare.com ", Contraseña = "Clave12345" });

        Assert.Equal("token-de-prueba", respuesta.Token);
        Assert.Equal(NombresRoles.Administrador, respuesta.Rol);
        _servicioToken.Verify(t => t.GenerarToken(It.IsAny<Usuario>()), Times.Once);
    }
}
