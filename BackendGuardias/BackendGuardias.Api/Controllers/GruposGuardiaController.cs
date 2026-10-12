using BackendGuardias.Api.DTOs.GruposGuardia;
using BackendGuardias.Api.Services.GruposGuardia;
using Microsoft.AspNetCore.Mvc;

namespace BackendGuardias.Api.Controllers;

[ApiController]
[Route("api/grupos-guardia")]
public sealed class GruposGuardiaController : ControllerBase
{
    private readonly IGrupoGuardiaService _service;

    public GruposGuardiaController(
        IGrupoGuardiaService service)
    {
        _service = service;
    }

    [HttpGet]
    [ProducesResponseType(typeof(GrupoGuardiaListaResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<GrupoGuardiaListaResponse>> ObtenerTodos(
        [FromQuery] bool incluirInactivos = false,
        CancellationToken cancellationToken = default)
    {
        var resultado =
            await _service.ObtenerTodosAsync(
                incluirInactivos,
                cancellationToken);

        return Ok(new GrupoGuardiaListaResponse(resultado.Count, resultado));
    }

    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(GrupoGuardiaDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<GrupoGuardiaDto>> ObtenerPorId(
        int id,
        CancellationToken cancellationToken = default)
    {
        var resultado =
            await _service.ObtenerPorIdAsync(
                id,
                cancellationToken);

        if (resultado is null)
        {
            return NotFound(new
            {
                message = "Grupo de guardia no encontrado."
            });
        }

        return Ok(resultado);
    }

    [HttpPost]
    [ProducesResponseType(typeof(GrupoGuardiaDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<GrupoGuardiaDto>> Crear(
        [FromBody] CrearGrupoGuardiaDto dto,
        CancellationToken cancellationToken = default)
    {
        var resultado =
            await _service.CrearAsync(
                dto,
                idPersonaActual: dto.IdPersonaCrea,
                cancellationToken);

        return CreatedAtAction(
            nameof(ObtenerPorId),
            new { id = resultado.IdGrupoGuardia },
            resultado);
    }

    [HttpPut("{id:int}")]
    [ProducesResponseType(typeof(GrupoGuardiaDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<GrupoGuardiaDto>> Actualizar(
        int id,
        [FromBody] ActualizarGrupoGuardiaDto dto,
        CancellationToken cancellationToken = default)
    {
        var resultado =
            await _service.ActualizarAsync(
                id,
                dto,
                idPersonaActual: dto.IdPersonaActualiza,
                cancellationToken);

        if (resultado is null)
        {
            return NotFound(new
            {
                message = "Grupo de guardia no encontrado."
            });
        }

        return Ok(resultado);
    }

    [HttpPatch("{id:int}/estado")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> CambiarEstado(
        int id,
        [FromQuery] bool activo,
        CancellationToken cancellationToken = default)
    {
        var resultado =
            await _service.CambiarEstadoAsync(
                id,
                activo,
                idPersonaActual: null,
                cancellationToken);

        if (!resultado)
        {
            return NotFound(new
            {
                message = "Grupo de guardia no encontrado."
            });
        }

        return Ok(new
        {
            success = true,
            activo
        });
    }
}