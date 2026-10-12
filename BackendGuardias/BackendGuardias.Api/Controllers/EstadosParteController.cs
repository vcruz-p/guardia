using BackendGuardias.Api.DTOs.Comun;
using BackendGuardias.Api.DTOs.EstadosParte;
using BackendGuardias.Api.Services.EstadosParte;
using Microsoft.AspNetCore.Mvc;

namespace BackendGuardias.Api.Controllers;

[ApiController]
[Route("api/estados-parte")]
public sealed class EstadosParteController : ControllerBase
{
    private readonly IEstadoParteService _service;

    public EstadosParteController(IEstadoParteService service)
    {
        _service = service;
    }

    /// <summary>
    /// Obtiene todos los estados, con filtros opcionales.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(
        typeof(ResultadoConsultaDto<EstadoParteDto>),
        StatusCodes.Status200OK)]
    public async Task<
        ActionResult<ResultadoConsultaDto<EstadoParteDto>>>
        ObtenerTodos(
            [FromQuery] string? buscar = null,
            [FromQuery] bool? activo = null,
            CancellationToken cancellationToken = default)
    {
        var resultado = await _service.ObtenerTodosAsync(
            buscar,
            activo,
            cancellationToken);

        return Ok(resultado);
    }

    /// <summary>
    /// Obtiene un estado por su identificador.
    /// </summary>
    [HttpGet("{idEstado:int}")]
    [ProducesResponseType(
        typeof(EstadoParteDto),
        StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<EstadoParteDto>> ObtenerPorId(
        [FromRoute] int idEstado,
        CancellationToken cancellationToken = default)
    {
        if (idEstado <= 0)
        {
            return BadRequest(new
            {
                success = false,
                mensaje = "El ID del estado debe ser mayor que cero."
            });
        }

        var estado = await _service.ObtenerPorIdAsync(
            idEstado,
            cancellationToken);

        if (estado is null)
        {
            return NotFound(new
            {
                success = false,
                mensaje = "El estado no existe."
            });
        }

        return Ok(estado);
    }

    /// <summary>
    /// Crea un nuevo estado.
    /// </summary>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Crear(
        [FromBody] CrearEstadoParteDto dto,
        [FromQuery] int idPersonaOperador,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var estado = await _service.CrearAsync(
                dto,
                idPersonaOperador,
                cancellationToken);

            return CreatedAtAction(
                nameof(ObtenerPorId),
                new { idEstado = estado.IdEstado },
                new
                {
                    success = true,
                    mensaje = "Estado creado correctamente.",
                    data = estado
                });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new
            {
                success = false,
                mensaje = ex.Message
            });
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new
            {
                success = false,
                mensaje = ex.Message
            });
        }
    }

    /// <summary>
    /// Actualiza los datos de un estado existente.
    /// </summary>
    [HttpPut("{idEstado:int}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Actualizar(
        [FromRoute] int idEstado,
        [FromBody] ActualizarEstadoParteDto dto,
        [FromQuery] int idPersonaOperador,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var estado = await _service.ActualizarAsync(
                idEstado,
                dto,
                idPersonaOperador,
                cancellationToken);

            if (estado is null)
            {
                return NotFound(new
                {
                    success = false,
                    mensaje = "El estado no existe."
                });
            }

            return Ok(new
            {
                success = true,
                mensaje = "Estado actualizado correctamente.",
                data = estado
            });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new
            {
                success = false,
                mensaje = ex.Message
            });
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new
            {
                success = false,
                mensaje = ex.Message
            });
        }
    }

    /// <summary>
    /// Desactiva un estado sin eliminarlo físicamente.
    /// </summary>
    [HttpDelete("{idEstado:int}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Desactivar(
        [FromRoute] int idEstado,
        [FromQuery] int idPersonaOperador,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var desactivado = await _service.DesactivarAsync(
                idEstado,
                idPersonaOperador,
                cancellationToken);

            if (!desactivado)
            {
                return NotFound(new
                {
                    success = false,
                    mensaje = "El estado no existe."
                });
            }

            return Ok(new
            {
                success = true,
                mensaje = "Estado desactivado correctamente.",
                idEstado
            });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new
            {
                success = false,
                mensaje = ex.Message
            });
        }
    }
}