namespace VetCare.Negocio.Excepciones;

/// <summary>Base de todas las excepciones controladas de la capa de negocio.</summary>
public abstract class ExcepcionNegocio : Exception
{
    protected ExcepcionNegocio(string mensaje) : base(mensaje)
    {
    }
}

/// <summary>El recurso solicitado no existe (HTTP 404).</summary>
public class ExcepcionNoEncontrada : ExcepcionNegocio
{
    public ExcepcionNoEncontrada(string mensaje) : base(mensaje)
    {
    }
}

/// <summary>El recurso ya existe o choca con otro (HTTP 409).</summary>
public class ExcepcionConflicto : ExcepcionNegocio
{
    public ExcepcionConflicto(string mensaje) : base(mensaje)
    {
    }
}

/// <summary>La peticion es valida pero incumple una regla de negocio (HTTP 422).</summary>
public class ExcepcionReglaNegocio : ExcepcionNegocio
{
    public ExcepcionReglaNegocio(string mensaje) : base(mensaje)
    {
    }
}

/// <summary>Credenciales incorrectas o usuario inactivo (HTTP 401).</summary>
public class ExcepcionNoAutenticado : ExcepcionNegocio
{
    public ExcepcionNoAutenticado(string mensaje) : base(mensaje)
    {
    }
}
