
using System.Globalization;
using BackendGuardias.Api.Data;
using BackendGuardias.Api.DTOs.GruposGuardia;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BackendGuardias.Api.Controllers;

[ApiController]
[Route("api/grupos-guardia/{idGrupoGuardia:int}/personas")]
public sealed class GrupoGuardiaPersonasController : ControllerBase
{
    private readonly GuardiasDbContext _context;

    public GrupoGuardiaPersonasController(GuardiasDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Asocia una persona a un grupo de guardia.
    /// Si tiene una asignación activa en otro grupo, la termina
    /// y crea una nueva conservando el historial y la auditoría.
    /// </summary>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> AsociarPersona(
        [FromRoute] int idGrupoGuardia,
        [FromBody] AsociarPersonaGrupoGuardiaDto dto,
        [FromQuery] int idPersonaOperador,
        CancellationToken cancellationToken)
    {
        if (idGrupoGuardia <= 0
            || dto.IdPersona <= 0
            || idPersonaOperador <= 0)
        {
            return BadRequest(new
            {
                success = false,
                mensaje =
                    "El grupo, la persona asignada y el operador deben tener identificadores válidos."
            });
        }

        var grupoExiste = await _context.Database
            .SqlQuery<long>($"""
                SELECT COUNT(*) AS Value
                FROM guardia.grupo_guardia
                WHERE id_grupo_guardia = {idGrupoGuardia}
                """)
            .SingleAsync(cancellationToken);

        if (grupoExiste == 0)
        {
            return NotFound(new
            {
                success = false,
                mensaje = "El grupo de guardia no existe."
            });
        }

        var personaExiste = await _context.Database
            .SqlQuery<long>($"""
                SELECT COUNT(*) AS Value
                FROM guardia.vista_personas_unidades_subordinacion
                WHERE id_persona = {dto.IdPersona}
                """)
            .SingleAsync(cancellationToken);

        if (personaExiste == 0)
        {
            return BadRequest(new
            {
                success = false,
                mensaje = "La persona no existe o no está disponible."
            });
        }

        var fechaInicio = dto.FechaInicio
            ?? DateOnly.FromDateTime(DateTime.Today);

        var fechaSql = fechaInicio.ToString(
            "yyyy-MM-dd",
            CultureInfo.InvariantCulture);

        if (fechaInicio < DateOnly.FromDateTime(DateTime.Today))
        {
            return BadRequest(new
            {
                success = false,
                mensaje =
                    "La fecha de inicio no puede ser anterior a la fecha actual."
            });
        }

        await using var transaction =
            await _context.Database.BeginTransactionAsync(
                cancellationToken);

        try
        {
            var asignacionActivaMismoGrupo = await _context.Database
                .SqlQuery<long>($"""
                    SELECT COUNT(*) AS Value
                    FROM guardia.grupo_guardia_persona
                    WHERE id_grupo_guardia = {idGrupoGuardia}
                      AND id_persona = {dto.IdPersona}
                      AND activo = 1
                      AND fecha_inicio <= {fechaSql}
                      AND (
                          fecha_fin IS NULL
                          OR fecha_fin >= {fechaSql}
                      )
                    """)
                .SingleAsync(cancellationToken);

            if (asignacionActivaMismoGrupo > 0)
            {
                await transaction.RollbackAsync(cancellationToken);

                return Conflict(new
                {
                    success = false,
                    mensaje =
                        "La persona ya tiene una asignación activa en este grupo."
                });
            }

            var asociacionExistente = await _context.Database
                .SqlQuery<long>($"""
                    SELECT COUNT(*) AS Value
                    FROM guardia.grupo_guardia_persona
                    WHERE id_grupo_guardia = {idGrupoGuardia}
                      AND id_persona = {dto.IdPersona}
                      AND fecha_inicio = {fechaSql}
                    """)
                .SingleAsync(cancellationToken);

            if (asociacionExistente > 0)
            {
                await transaction.RollbackAsync(cancellationToken);

                return Conflict(new
                {
                    success = false,
                    mensaje =
                        "Ya existe una asociación para esa persona, grupo y fecha de inicio."
                });
            }

            // Cerrar las asignaciones activas en otros grupos.
            // El operador que efectúa este cambio queda registrado
            // como la persona que actualizó el registro histórico.
            var asignacionesTerminadas =
                await _context.Database.ExecuteSqlInterpolatedAsync($"""
                    UPDATE guardia.grupo_guardia_persona
                    SET fecha_fin = {fechaSql},
                        activo = 0,
                        id_persona_actualiza = {idPersonaOperador},
                        fecha_hora_actualizado = CURRENT_TIMESTAMP
                    WHERE id_persona = {dto.IdPersona}
                      AND id_grupo_guardia <> {idGrupoGuardia}
                      AND activo = 1
                      AND fecha_inicio <= {fechaSql}
                      AND (
                          fecha_fin IS NULL
                          OR fecha_fin >= {fechaSql}
                      )
                    """, cancellationToken);

            // Crear una nueva fila con la auditoría de creación.
            await _context.Database.ExecuteSqlInterpolatedAsync($"""
                INSERT INTO guardia.grupo_guardia_persona
                (
                    id_grupo_guardia,
                    id_persona,
                    fecha_inicio,
                    fecha_fin,
                    activo,
                    observacion,
                    id_persona_crea,
                    id_persona_actualiza,
                    fecha_hora_creado,
                    fecha_hora_actualizado
                )
                VALUES
                (
                    {idGrupoGuardia},
                    {dto.IdPersona},
                    {fechaSql},
                    NULL,
                    1,
                    {dto.Observacion},
                    {idPersonaOperador},
                    NULL,
                    CURRENT_TIMESTAMP,
                    CURRENT_TIMESTAMP
                )
                """, cancellationToken);

            var idAsociacion = await _context.Database
                .SqlQuery<int>($"""
                    SELECT id_grupo_guardia_persona AS Value
                    FROM guardia.grupo_guardia_persona
                    WHERE id_grupo_guardia = {idGrupoGuardia}
                      AND id_persona = {dto.IdPersona}
                      AND fecha_inicio = {fechaSql}
                    ORDER BY id_grupo_guardia_persona DESC
                    LIMIT 1
                    """)
                .SingleAsync(cancellationToken);

            await transaction.CommitAsync(cancellationToken);

            return StatusCode(StatusCodes.Status201Created, new
            {
                success = true,
                mensaje =
                    "Persona asignada correctamente al grupo de guardia.",
                data = new
                {
                    idGrupoGuardiaPersona = idAsociacion,
                    idGrupoGuardia,
                    idPersona = dto.IdPersona,
                    fechaInicio,
                    fechaFin = (DateOnly?)null,
                    activo = true,
                    observacion = dto.Observacion,
                    idPersonaCrea = idPersonaOperador,
                    idPersonaActualiza = (int?)null,
                    asignacionesAnterioresTerminadas =
                        asignacionesTerminadas
                }
            });
        }
        catch
        {
            await transaction.RollbackAsync(cancellationToken);
            throw;
        }
    }

    /// <summary>
    /// Termina la asignación activa de una persona en este grupo.
    /// Conserva el historial y registra quién realizó el cambio.
    /// </summary>
    [HttpDelete("{idPersona:int}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> QuitarAsignacion(
        [FromRoute] int idGrupoGuardia,
        [FromRoute] int idPersona,
        [FromQuery] int idPersonaOperador,
        CancellationToken cancellationToken)
    {
        if (idGrupoGuardia <= 0
            || idPersona <= 0
            || idPersonaOperador <= 0)
        {
            return BadRequest(new
            {
                success = false,
                mensaje =
                    "El grupo, la persona asignada y el operador deben tener identificadores válidos."
            });
        }

        var fechaHoy = DateOnly.FromDateTime(DateTime.Today);

        var fechaSql = fechaHoy.ToString(
            "yyyy-MM-dd",
            CultureInfo.InvariantCulture);

        var filasActualizadas = await _context.Database
            .ExecuteSqlInterpolatedAsync($"""
                UPDATE guardia.grupo_guardia_persona
                SET fecha_fin = {fechaSql},
                    activo = 0,
                    id_persona_actualiza = {idPersonaOperador},
                    fecha_hora_actualizado = CURRENT_TIMESTAMP
                WHERE id_grupo_guardia = {idGrupoGuardia}
                  AND id_persona = {idPersona}
                  AND activo = 1
                  AND fecha_inicio <= {fechaSql}
                  AND (
                      fecha_fin IS NULL
                      OR fecha_fin >= {fechaSql}
                  )
                """, cancellationToken);

        if (filasActualizadas == 0)
        {
            return NotFound(new
            {
                success = false,
                mensaje =
                    "No se encontró una asignación activa de esa persona en este grupo."
            });
        }

        return Ok(new
        {
            success = true,
            mensaje = "La asignación fue terminada correctamente.",
            data = new
            {
                idGrupoGuardia,
                idPersona,
                fechaFin = fechaHoy,
                activo = false,
                idPersonaActualiza = idPersonaOperador
            }
        });
    }
}