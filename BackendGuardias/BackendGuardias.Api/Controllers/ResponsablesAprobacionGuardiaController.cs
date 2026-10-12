
using BackendGuardias.Api.DTOs.Comun;
using BackendGuardias.Api.DTOs.ResponsablesAprobacionGuardia;
using BackendGuardias.Api.Services.ResponsablesAprobacionGuardia;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BackendGuardias.Api.Controllers;

[ApiController]
[Route("api/responsables-aprobacion-guardia")]
public sealed class ResponsablesAprobacionGuardiaController : ControllerBase
{
    private readonly IResponsableAprobacionGuardiaService _service;

    public ResponsablesAprobacionGuardiaController(
        IResponsableAprobacionGuardiaService service)
    {
        _service = service;
    }

    // ============================================================
    // PLANIFICADORES
    // ============================================================

    /// <summary>
    /// Obtiene los planificadores, con filtros opcionales.
    /// </summary>
    [HttpGet("planificadores")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public Task<IActionResult> ObtenerPlanificadores(
        [FromQuery] int? idUnidadOrganizativa = null,
        [FromQuery] int? idPersona = null,
        [FromQuery] string? buscar = null,
        CancellationToken cancellationToken = default)
    {
        return ObtenerTodosPorTipo(
            TipoResponsableAprobacionGuardia.Planificador,
            idUnidadOrganizativa,
            idPersona,
            buscar,
            cancellationToken);
    }

    /// <summary>
    /// Obtiene un planificador por su identificador.
    /// </summary>
    [HttpGet("planificadores/{idResponsable:int}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public Task<IActionResult> ObtenerPlanificador(
        [FromRoute] int idResponsable,
        CancellationToken cancellationToken = default)
    {
        return ObtenerPorIdYTipo(
            idResponsable,
            TipoResponsableAprobacionGuardia.Planificador,
            cancellationToken);
    }

    /// <summary>
    /// Crea un planificador para una unidad organizativa.
    /// </summary>
    [HttpPost("planificadores")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public Task<IActionResult> CrearPlanificador(
        [FromBody] CrearResponsableAprobacionGuardiaDto dto,
        [FromQuery] int idPersonaOperador,
        CancellationToken cancellationToken = default)
    {
        return CrearPorTipo(
            dto,
            TipoResponsableAprobacionGuardia.Planificador,
            idPersonaOperador,
            cancellationToken);
    }

    /// <summary>
    /// Actualiza un planificador existente.
    /// </summary>
    [HttpPut("planificadores/{idResponsable:int}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public Task<IActionResult> ActualizarPlanificador(
        [FromRoute] int idResponsable,
        [FromBody] ActualizarResponsableAprobacionGuardiaDto dto,
        CancellationToken cancellationToken = default)
    {
        return ActualizarPorTipo(
            idResponsable,
            dto,
            TipoResponsableAprobacionGuardia.Planificador,
            cancellationToken);
    }

    /// <summary>
    /// Elimina un planificador.
    /// </summary>
    [HttpDelete("planificadores/{idResponsable:int}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public Task<IActionResult> EliminarPlanificador(
        [FromRoute] int idResponsable,
        CancellationToken cancellationToken = default)
    {
        return EliminarPorTipo(
            idResponsable,
            TipoResponsableAprobacionGuardia.Planificador,
            cancellationToken);
    }

    // ============================================================
    // APROBADORES
    // ============================================================

    /// <summary>
    /// Obtiene los aprobadores, con filtros opcionales.
    /// </summary>
    [HttpGet("aprobadores")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public Task<IActionResult> ObtenerAprobadores(
        [FromQuery] int? idUnidadOrganizativa = null,
        [FromQuery] int? idPersona = null,
        [FromQuery] string? buscar = null,
        CancellationToken cancellationToken = default)
    {
        return ObtenerTodosPorTipo(
            TipoResponsableAprobacionGuardia.Aprueba,
            idUnidadOrganizativa,
            idPersona,
            buscar,
            cancellationToken);
    }

    /// <summary>
    /// Obtiene un aprobador por su identificador.
    /// </summary>
    [HttpGet("aprobadores/{idResponsable:int}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public Task<IActionResult> ObtenerAprobador(
        [FromRoute] int idResponsable,
        CancellationToken cancellationToken = default)
    {
        return ObtenerPorIdYTipo(
            idResponsable,
            TipoResponsableAprobacionGuardia.Aprueba,
            cancellationToken);
    }

    /// <summary>
    /// Crea un aprobador para una unidad organizativa.
    /// </summary>
    [HttpPost("aprobadores")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public Task<IActionResult> CrearAprobador(
        [FromBody] CrearResponsableAprobacionGuardiaDto dto,
        [FromQuery] int idPersonaOperador,
        CancellationToken cancellationToken = default)
    {
        return CrearPorTipo(
            dto,
            TipoResponsableAprobacionGuardia.Aprueba,
            idPersonaOperador,
            cancellationToken);
    }

    /// <summary>
    /// Actualiza un aprobador existente.
    /// </summary>
    [HttpPut("aprobadores/{idResponsable:int}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public Task<IActionResult> ActualizarAprobador(
        [FromRoute] int idResponsable,
        [FromBody] ActualizarResponsableAprobacionGuardiaDto dto,
        CancellationToken cancellationToken = default)
    {
        return ActualizarPorTipo(
            idResponsable,
            dto,
            TipoResponsableAprobacionGuardia.Aprueba,
            cancellationToken);
    }

    /// <summary>
    /// Elimina un aprobador.
    /// </summary>
    [HttpDelete("aprobadores/{idResponsable:int}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public Task<IActionResult> EliminarAprobador(
        [FromRoute] int idResponsable,
        CancellationToken cancellationToken = default)
    {
        return EliminarPorTipo(
            idResponsable,
            TipoResponsableAprobacionGuardia.Aprueba,
            cancellationToken);
    }

    // ============================================================
    // RESPONSABLES POR UNIDAD ORGANIZATIVA
    // ============================================================

    /// <summary>
    /// Obtiene los planificadores y aprobadores de una unidad organizativa.
    /// </summary>
    [HttpGet("unidad/{idUnidadOrganizativa:int}")]
    [ProducesResponseType(
        typeof(ResultadoConsultaDto<ResponsableAprobacionGuardiaDto>),
        StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<
        ActionResult<ResultadoConsultaDto<ResponsableAprobacionGuardiaDto>>>
        ObtenerResponsablesPorUnidad(
            [FromRoute] int idUnidadOrganizativa,
            CancellationToken cancellationToken = default)
    {
        if (idUnidadOrganizativa <= 0)
        {
            return BadRequest(new
            {
                success = false,
                mensaje =
                    "El ID de unidad organizativa debe ser mayor que cero."
            });
        }

        try
        {
            var planificadores = await _service.ObtenerTodosAsync(
                TipoResponsableAprobacionGuardia.Planificador,
                idUnidadOrganizativa,
                null,
                null,
                cancellationToken);

            var aprobadores = await _service.ObtenerTodosAsync(
                TipoResponsableAprobacionGuardia.Aprueba,
                idUnidadOrganizativa,
                null,
                null,
                cancellationToken);

            var responsables = planificadores.Data
                .Concat(aprobadores.Data)
                .OrderBy(x => x.Tipo)
                .ThenBy(x => x.PersonaNombreCompleto)
                .ToList();

            var resultado =
                new ResultadoConsultaDto<ResponsableAprobacionGuardiaDto>
                {
                    Total = responsables.Count,
                    Data = responsables
                };

            return Ok(resultado);
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

    // ============================================================
    // MÉTODOS COMPARTIDOS
    // ============================================================

    private async Task<IActionResult> ObtenerTodosPorTipo(
        string tipo,
        int? idUnidadOrganizativa,
        int? idPersona,
        string? buscar,
        CancellationToken cancellationToken)
    {
        try
        {
            var resultado = await _service.ObtenerTodosAsync(
                tipo,
                idUnidadOrganizativa,
                idPersona,
                buscar,
                cancellationToken);

            return Ok(resultado);
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

    private async Task<IActionResult> ObtenerPorIdYTipo(
        int idResponsable,
        string tipo,
        CancellationToken cancellationToken)
    {
        try
        {
            var resultado = await _service.ObtenerPorIdAsync(
                idResponsable,
                tipo,
                cancellationToken);

            if (resultado is null)
            {
                return NotFound(new
                {
                    success = false,
                    mensaje = "El responsable no existe para este tipo."
                });
            }

            return Ok(resultado);
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

    private async Task<IActionResult> CrearPorTipo(
        CrearResponsableAprobacionGuardiaDto dto,
        string tipo,
        int idPersonaOperador,
        CancellationToken cancellationToken)
    {
        try
        {
            var resultado = await _service.CrearAsync(
                dto,
                tipo,
                idPersonaOperador,
                cancellationToken);

            return StatusCode(StatusCodes.Status201Created, new
            {
                success = true,
                mensaje = "Responsable registrado correctamente.",
                data = resultado
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
        catch (DbUpdateException)
        {
            return Conflict(new
            {
                success = false,
                mensaje =
                    "No se pudo crear el responsable. Comprueba que no exista otro del mismo tipo en la unidad."
            });
        }
    }

    private async Task<IActionResult> ActualizarPorTipo(
        int idResponsable,
        ActualizarResponsableAprobacionGuardiaDto dto,
        string tipo,
        CancellationToken cancellationToken)
    {
        try
        {
            var resultado = await _service.ActualizarAsync(
                idResponsable,
                dto,
                tipo,
                cancellationToken);

            if (resultado is null)
            {
                return NotFound(new
                {
                    success = false,
                    mensaje = "El responsable no existe para este tipo."
                });
            }

            return Ok(new
            {
                success = true,
                mensaje = "Responsable actualizado correctamente.",
                data = resultado
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
        catch (DbUpdateException)
        {
            return Conflict(new
            {
                success = false,
                mensaje =
                    "No se pudo actualizar el responsable. Comprueba las restricciones de la base de datos."
            });
        }
    }

    private async Task<IActionResult> EliminarPorTipo(
        int idResponsable,
        string tipo,
        CancellationToken cancellationToken)
    {
        try
        {
            var eliminado = await _service.EliminarAsync(
                idResponsable,
                tipo,
                cancellationToken);

            if (!eliminado)
            {
                return NotFound(new
                {
                    success = false,
                    mensaje = "El responsable no existe para este tipo."
                });
            }

            return Ok(new
            {
                success = true,
                mensaje = "Responsable eliminado correctamente.",
                idResponsable,
                tipo
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
        catch (DbUpdateException)
        {
            return Conflict(new
            {
                success = false,
                mensaje =
                    "No se puede eliminar el responsable porque existen registros relacionados."
            });
        }
    }
}