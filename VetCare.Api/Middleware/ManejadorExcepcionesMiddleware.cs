using VetCare.Api.Modelos;
using VetCare.Negocio.Excepciones;

namespace VetCare.Api.Middleware;

/// <summary>Traduce las excepciones de la capa de negocio a codigos HTTP.</summary>
public class ManejadorExcepcionesMiddleware
{
    private readonly RequestDelegate _siguiente;
    private readonly ILogger<ManejadorExcepcionesMiddleware> _registro;

    public ManejadorExcepcionesMiddleware(RequestDelegate siguiente, ILogger<ManejadorExcepcionesMiddleware> registro)
    {
        _siguiente = siguiente;
        _registro = registro;
    }

    public async Task InvokeAsync(HttpContext contexto)
    {
        try
        {
            await _siguiente(contexto);
        }
        catch (ExcepcionNegocio ex)
        {
            var estado = ex switch
            {
                ExcepcionNoEncontrada => StatusCodes.Status404NotFound,
                ExcepcionConflicto => StatusCodes.Status409Conflict,
                ExcepcionReglaNegocio => StatusCodes.Status422UnprocessableEntity,
                ExcepcionNoAutenticado => StatusCodes.Status401Unauthorized,
                _ => StatusCodes.Status400BadRequest
            };
            await Responder(contexto, estado, ex.Message);
        }
        catch (Exception ex)
        {
            _registro.LogError(ex, "Error no controlado");
            await Responder(contexto, StatusCodes.Status500InternalServerError,
                "Ocurrio un error inesperado en el servidor");
        }
    }

    private static async Task Responder(HttpContext contexto, int estado, string mensaje)
    {
        if (contexto.Response.HasStarted)
        {
            return;
        }

        contexto.Response.Clear();
        contexto.Response.StatusCode = estado;
        await contexto.Response.WriteAsJsonAsync(new RespuestaError { Estado = estado, Mensaje = mensaje });
    }
}
