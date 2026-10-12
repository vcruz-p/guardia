using BackendGuardias.Api.DTOs.GruposGuardia;

namespace BackendGuardias.Api.Services.GruposGuardia;

public interface IGrupoGuardiaService
{
    Task<IReadOnlyList<GrupoGuardiaDto>> ObtenerTodosAsync(
        bool incluirInactivos,
        CancellationToken cancellationToken);

    Task<GrupoGuardiaDto?> ObtenerPorIdAsync(
        int id,
        CancellationToken cancellationToken);

    Task<GrupoGuardiaDto> CrearAsync(
        CrearGrupoGuardiaDto dto,
        int? idPersonaActual,
        CancellationToken cancellationToken);

    Task<GrupoGuardiaDto?> ActualizarAsync(
        int id,
        ActualizarGrupoGuardiaDto dto,
        int? idPersonaActual,
        CancellationToken cancellationToken);

    Task<bool> CambiarEstadoAsync(
        int id,
        bool activo,
        int? idPersonaActual,
        CancellationToken cancellationToken);
}