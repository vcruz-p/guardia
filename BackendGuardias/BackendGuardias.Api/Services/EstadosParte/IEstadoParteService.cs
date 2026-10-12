using BackendGuardias.Api.DTOs.Comun;
using BackendGuardias.Api.DTOs.EstadosParte;

namespace BackendGuardias.Api.Services.EstadosParte;

public interface IEstadoParteService
{
    Task<ResultadoConsultaDto<EstadoParteDto>> ObtenerTodosAsync(
        string? buscar,
        bool? activo,
        CancellationToken cancellationToken = default);

    Task<EstadoParteDto?> ObtenerPorIdAsync(
        int idEstado,
        CancellationToken cancellationToken = default);

    Task<EstadoParteDto> CrearAsync(
        CrearEstadoParteDto dto,
        int idPersonaOperador,
        CancellationToken cancellationToken = default);

    Task<EstadoParteDto?> ActualizarAsync(
        int idEstado,
        ActualizarEstadoParteDto dto,
        int idPersonaOperador,
        CancellationToken cancellationToken = default);

    Task<bool> DesactivarAsync(
        int idEstado,
        int idPersonaOperador,
        CancellationToken cancellationToken = default);
}