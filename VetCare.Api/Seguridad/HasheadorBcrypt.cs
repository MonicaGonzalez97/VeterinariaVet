using VetCare.Negocio.Servicios;

namespace VetCare.Api.Seguridad;

public class HasheadorBcrypt : IHasheador
{
    public string HashearContraseña(string contraseña)
    {
        return BCrypt.Net.BCrypt.HashPassword(contraseña);
    }

    public bool VerificarContraseña(string contraseña, string hash)
    {
        return BCrypt.Net.BCrypt.Verify(contraseña, hash);
    }
}
