using VetCare.Datos.Entidades;
using VetCare.Datos.Repositorios;
using VetCare.Negocio.Constantes;
using VetCare.Negocio.Dtos;
using VetCare.Negocio.Excepciones;

namespace VetCare.Negocio.Servicios;

public class ServicioAutenticacion : IServicioAutenticacion
{
    private readonly IRepositorioUsuario _usuarios;
    private readonly IRepositorioRol _roles;
    private readonly IHasheador _hasheador;
    private readonly IServicioToken _servicioToken;

    public ServicioAutenticacion(
        IRepositorioUsuario usuarios,
        IRepositorioRol roles,
        IHasheador hasheador,
        IServicioToken servicioToken)
    {
        _usuarios = usuarios;
        _roles = roles;
        _hasheador = hasheador;
        _servicioToken = servicioToken;
    }

    public async Task<RespuestaAutenticacionDto> RegistrarAsync(RegistroDto dto)
    {
        var correo = dto.Correo.Trim().ToLowerInvariant();

        if (await _usuarios.BuscarPorCorreoAsync(correo) != null)
        {
            throw new ExcepcionConflicto("Ya existe un usuario registrado con ese correo");
        }

        // El registro publico siempre crea usuarios con el rol de menor privilegio
        var rol = await _roles.BuscarPorNombreAsync(NombresRoles.Recepcionista)
            ?? throw new ExcepcionReglaNegocio("El rol por defecto no esta configurado");

        var usuario = new Usuario
        {
            Nombre = dto.Nombre.Trim(),
            Correo = correo,
            HashContraseña = _hasheador.HashearContraseña(dto.Contraseña),
            RolId = rol.Id,
            Rol = rol,
            Activo = true
        };

        await _usuarios.CrearAsync(usuario);
        return _servicioToken.GenerarToken(usuario);
    }

    public async Task<RespuestaAutenticacionDto> IniciarSesionAsync(LoginDto dto)
    {
        var correo = dto.Correo.Trim().ToLowerInvariant();
        var usuario = await _usuarios.BuscarPorCorreoAsync(correo);

        // Mismo mensaje en todos los casos para no revelar si el correo existe
        if (usuario == null
            || !usuario.Activo
            || !_hasheador.VerificarContraseña(dto.Contraseña, usuario.HashContraseña))
        {
            throw new ExcepcionNoAutenticado("Correo o contraseña incorrectos");
        }

        return _servicioToken.GenerarToken(usuario);
    }
}
