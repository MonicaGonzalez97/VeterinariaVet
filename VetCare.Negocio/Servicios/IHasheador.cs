namespace VetCare.Negocio.Servicios;

public interface IHasheador
{
    string HashearContraseña(string contraseña);
    bool VerificarContraseña(string contraseña, string hash);
}
