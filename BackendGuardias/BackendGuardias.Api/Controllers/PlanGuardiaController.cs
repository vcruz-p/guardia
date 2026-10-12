using System.Security.Claims;
using BackendGuardias.Api.DTOs.PlanGuardia;
using BackendGuardias.Api.Models.Views;
using BackendGuardias.Api.Services.PlanGuardia;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BackendGuardias.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public sealed class PlanGuardiaController : ControllerBase
{
    private readonly IPlanGuardiaService _service;
    private readonly ILogger<PlanGuardiaController> _logger;

    public PlanGuardiaController(
        IPlanGuardiaService service,
        ILogger<PlanGuardiaController> logger)
    {
        _service = service;
        _logger = logger;
    }

    // =========================================================
    // GET: api/PlanGuardia
    // Listar planes con filtros opcionales.
    // =========================================================

    [HttpGet]
    [ProducesResponseType(
        typeof(IReadOnlyList<VistaPlanGuardia>),
        StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<IReadOnlyList<VistaPlanGuardia>>>
        ObtenerTodos(
            [FromQuery] int? idEstado = null,
            [FromQuery] DateTime? fechaDesde = null,
            [FromQuery] DateTime? fechaHasta = null)
    {
        try
        {
            var resultado = await _service.ObtenerTodosAsync(
                idEstado,
                fechaDesde,
                fechaHasta);

            return Ok(resultado);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { mensaje = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Error al consultar los planes de guardia.");

            return Problem(
                title: "Error al consultar los planes de guardia.",
                statusCode: StatusCodes.Status500InternalServerError);
        }
    }

    // =========================================================
    // GET: api/PlanGuardia/5
    // Obtener un plan por ID.
    // =========================================================

    [HttpGet("{idPlanGuardia:int}")]
    [ProducesResponseType(
        typeof(VistaPlanGuardia),
        StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<VistaPlanGuardia>> ObtenerPorId(
        int idPlanGuardia)
    {
        try
        {
            var resultado = await _service.ObtenerPorIdAsync(
                idPlanGuardia);

            return Ok(resultado);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { mensaje = ex.Message });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { mensaje = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Error al consultar el plan {IdPlanGuardia}.",
                idPlanGuardia);

            return Problem(
                title: "Error al consultar el plan de guardia.",
                statusCode: StatusCodes.Status500InternalServerError);
        }
    }

    // =========================================================
    // POST: api/PlanGuardia
    // Crear un plan con sus detalles.
    // El creador procede del usuario autenticado.
    // =========================================================

    [HttpPost]
    [ProducesResponseType(
        typeof(VistaPlanGuardia),
        StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<VistaPlanGuardia>> Crear(
        [FromBody] PlanGuardiaCrearDto dto)
    {
        if (!TryObtenerIdPersona(out var idPersona))
        {
            return Unauthorized(new
            {
                mensaje = "No se pudo identificar a la persona autenticada."
            });
        }

        try
        {
            var resultado = await _service.CrearAsync(
                dto,
                idPersona);

            return CreatedAtAction(
                nameof(ObtenerPorId),
                new { idPlanGuardia = resultado.IdPlanGuardia },
                resultado);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { mensaje = ex.Message });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { mensaje = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { mensaje = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Error al crear el plan de guardia.");

            return Problem(
                title: "Error al crear el plan de guardia.",
                statusCode: StatusCodes.Status500InternalServerError);
        }
    }

    // =========================================================
    // PUT: api/PlanGuardia/5
    // Actualizar el encabezado y reemplazar sus detalles.
    // =========================================================

    [HttpPut("{idPlanGuardia:int}")]
    [ProducesResponseType(
        typeof(VistaPlanGuardia),
        StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<VistaPlanGuardia>> Actualizar(
        int idPlanGuardia,
        [FromBody] PlanGuardiaActualizarDto dto)
    {
        if (!TryObtenerIdPersona(out var idPersona))
        {
            return Unauthorized(new
            {
                mensaje = "No se pudo identificar a la persona autenticada."
            });
        }

        try
        {
            var resultado = await _service.ActualizarAsync(
                idPlanGuardia,
                dto,
                idPersona);

            return Ok(resultado);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { mensaje = ex.Message });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { mensaje = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { mensaje = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Error al actualizar el plan {IdPlanGuardia}.",
                idPlanGuardia);

            return Problem(
                title: "Error al actualizar el plan de guardia.",
                statusCode: StatusCodes.Status500InternalServerError);
        }
    }

    // =========================================================
    // DELETE: api/PlanGuardia/5
    // Eliminar un plan no aprobado.
    // =========================================================

    [HttpDelete("{idPlanGuardia:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Eliminar(
        int idPlanGuardia)
    {
        try
        {
            await _service.EliminarAsync(idPlanGuardia);

            return NoContent();
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { mensaje = ex.Message });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { mensaje = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { mensaje = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Error al eliminar el plan {IdPlanGuardia}.",
                idPlanGuardia);

            return Problem(
                title: "Error al eliminar el plan de guardia.",
                statusCode: StatusCodes.Status500InternalServerError);
        }
    }

    // =========================================================
    // POST: api/PlanGuardia/5/aprobar
    // Cambiar el estado del plan a APROBADO.
    // =========================================================

    [HttpPost("{idPlanGuardia:int}/aprobar")]
    [ProducesResponseType(
        typeof(VistaPlanGuardia),
        StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<VistaPlanGuardia>> Aprobar(
        int idPlanGuardia)
    {
        if (!TryObtenerIdPersona(out var idPersona))
        {
            return Unauthorized(new
            {
                mensaje = "No se pudo identificar a la persona autenticada."
            });
        }

        try
        {
            var resultado = await _service.AprobarAsync(
                idPlanGuardia,
                idPersona);

            return Ok(resultado);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { mensaje = ex.Message });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { mensaje = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { mensaje = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Error al aprobar el plan {IdPlanGuardia}.",
                idPlanGuardia);

            return Problem(
                title: "Error al aprobar el plan de guardia.",
                statusCode: StatusCodes.Status500InternalServerError);
        }
    }

    // =========================================================
    // POST: api/PlanGuardia/planificacion-automatica
    // Genera un plan automático para el período comprendido
    // entre la fecha inicial y la fecha final recibidas.
    // Si algún día del período ya tiene plan, devuelve 409.
    // =========================================================

    [HttpPost("planificacion-automatica")]
    [ProducesResponseType(
        typeof(VistaPlanGuardia),
        StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<VistaPlanGuardia>>
        PlanificarAutomaticamente(
            [FromBody] PlanGuardiaPlanificacionAutomaticaDto dto)
    {
        if (!TryObtenerIdPersona(out var idPersona))
        {
            return Unauthorized(new
            {
                mensaje = "No se pudo identificar a la persona autenticada."
            });
        }

        try
        {
            var resultado =
                await _service.PlanificarAutomaticamenteAsync(
                    dto,
                    idPersona);

            return CreatedAtAction(
                nameof(ObtenerPorId),
                new { idPlanGuardia = resultado.IdPlanGuardia },
                resultado);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { mensaje = ex.Message });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { mensaje = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { mensaje = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Error al generar el plan automático para {FechaInicial:yyyy-MM-dd} - {FechaFin:yyyy-MM-dd}.",
                dto?.FechaInicial ?? default,
                dto?.FechaFin ?? default);

            return Problem(
                title: "Error al generar el plan automático.",
                statusCode: StatusCodes.Status500InternalServerError);
        }
    }

    // =========================================================
    // OBTENER ID DE PERSONA AUTENTICADA
    // =========================================================

    private bool TryObtenerIdPersona(out int idPersona)
    {
        idPersona = 0;

        string[] nombresClaims =
        {
            "id_persona",
            "IdPersona",
            "idPersona",
            ClaimTypes.NameIdentifier,
            "sub"
        };

        foreach (var nombreClaim in nombresClaims)
        {
            var valor = User.FindFirst(nombreClaim)?.Value;

            if (int.TryParse(valor, out var id) && id > 0)
            {
                idPersona = id;
                return true;
            }
        }

        return false;
    }
}