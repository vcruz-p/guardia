
using BackendGuardias.Api.DTOs.Comun;
using BackendGuardias.Api.DTOs.PersonasUnidadOrganizativa;
using BackendGuardias.Api.Models.Views;
using BackendGuardias.Api.Services.PersonasUnidadOrganizativa;
using Microsoft.AspNetCore.Mvc;

namespace BackendGuardias.Api.Controllers;

[ApiController]
[Route("api/unidades-organizativas")]
public sealed class PersonasUnidadOrganizativaController : ControllerBase
{
    private readonly IPersonasUnidadOrganizativaService _service;

    public PersonasUnidadOrganizativaController(
        IPersonasUnidadOrganizativaService service)
    {
        _service = service;
    }

    /// <summary>
    /// Obtiene las personas de las unidades subordinadas directas
    /// e indirectas de una unidad organizativa.
    /// </summary>
    [HttpGet("{idUnidadOrganizativa:int}/personas-subordinadas")]
    [ProducesResponseType(
        typeof(ResultadoConsultaDto<PersonaUnidadSubordinacionView>),
        StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<
        ActionResult<ResultadoConsultaDto<PersonaUnidadSubordinacionView>>>
        ObtenerSubordinadas(
            [FromRoute] int idUnidadOrganizativa,
            [FromQuery] string? nombre,
            [FromQuery] string? nombreUnidadOrganizativa,
            [FromQuery] bool incluirPersonasDeLaUnidad = false,
            CancellationToken cancellationToken = default)
    {
        if (idUnidadOrganizativa <= 0)
        {
            return BadRequest(new
            {
                mensaje =
                    "El ID de unidad organizativa debe ser mayor que cero."
            });
        }

        var resultado = await _service.ObtenerSubordinadasAsync(
            idUnidadOrganizativa,
            nombre,
            nombreUnidadOrganizativa,
            incluirPersonasDeLaUnidad,
            cancellationToken);

        return Ok(resultado);
    }

    /// <summary>
    /// Obtiene las personas asignadas y sin asignar al grupo de guardia
    /// seleccionado dentro de una unidad organizativa y sus subordinadas.
    /// </summary>
    [HttpGet(
        "{idUnidadOrganizativa:int}/grupos/{idGrupoGuardia:int}/personas")]
    [ProducesResponseType(
        typeof(ResultadoPersonasGrupoGuardiaDto),
        StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ResultadoPersonasGrupoGuardiaDto>>
        ObtenerPersonasPorUnidadYGrupo(
            [FromRoute] int idUnidadOrganizativa,
            [FromRoute] int idGrupoGuardia,
            [FromQuery] string? nombreAsignadas = null,
            [FromQuery] string? nombreUnidadOrganizativaAsignadas = null,
            [FromQuery] string? nombreSinAsignar = null,
            [FromQuery] string? nombreUnidadOrganizativaSinAsignar = null,
            [FromQuery] bool incluirPersonasDeLaUnidad = false,
            CancellationToken cancellationToken = default)
    {
        if (idUnidadOrganizativa <= 0)
        {
            return BadRequest(new
            {
                mensaje =
                    "El ID de unidad organizativa debe ser mayor que cero."
            });
        }

        if (idGrupoGuardia <= 0)
        {
            return BadRequest(new
            {
                mensaje =
                    "El ID del grupo de guardia debe ser mayor que cero."
            });
        }

        var resultado =
            await _service.ObtenerPersonasPorUnidadYGrupoAsync(
                idUnidadOrganizativa,
                idGrupoGuardia,
                nombreAsignadas,
                nombreUnidadOrganizativaAsignadas,
                nombreSinAsignar,
                nombreUnidadOrganizativaSinAsignar,
                incluirPersonasDeLaUnidad,
                cancellationToken);

        return Ok(resultado);
    }

    /// <summary>
/// Obtiene el árbol de unidades organizativas y, opcionalmente,
/// las personas de cada unidad. Permite filtrar por persona y unidad.
/// </summary>
[HttpGet("arbol")]
[ProducesResponseType(StatusCodes.Status200OK)]
[ProducesResponseType(StatusCodes.Status400BadRequest)]
public async Task<
    ActionResult<IReadOnlyList<UnidadOrganizativaArbolDto>>>
    ObtenerArbolCompleto(
        [FromQuery] int? idUnidadOrganizativa = null,
        [FromQuery] string? nombrePersona = null,
        [FromQuery] string? nombreUnidadOrganizativa = null,
        [FromQuery] bool incluirPersonas = true,
        CancellationToken cancellationToken = default)
{
    try
    {
        var resultado = await _service.ObtenerArbolCompletoAsync(
            idUnidadOrganizativa,
            nombrePersona,
            nombreUnidadOrganizativa,
            incluirPersonas,
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
}