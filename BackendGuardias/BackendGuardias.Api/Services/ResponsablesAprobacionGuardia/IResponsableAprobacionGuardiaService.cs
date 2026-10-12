using BackendGuardias.Api.DTOs.Comun;
using BackendGuardias.Api.DTOs.ResponsablesAprobacionGuardia;

namespace BackendGuardias.Api.Services.ResponsablesAprobacionGuardia;

public interface IResponsableAprobacionGuardiaService
{
    Task<ResultadoConsultaDto<ResponsableAprobacionGuardiaDto>>
        ObtenerTodosAsync(
            string tipo,
            int? idUnidadOrganizativa,
            int? idPersona,
            string? buscar,
            CancellationToken cancellationToken = default);

    Task<ResponsableAprobacionGuardiaDto?> ObtenerPorIdAsync(
        int idResponsable,
        string tipo,
        CancellationToken cancellationToken = default);

    Task<ResponsableAprobacionGuardiaDto> CrearAsync(
        CrearResponsableAprobacionGuardiaDto dto,
        string tipo,
        int idPersonaOperador,
        CancellationToken cancellationToken = default);

    Task<ResponsableAprobacionGuardiaDto?> ActualizarAsync(
        int idResponsable,
        ActualizarResponsableAprobacionGuardiaDto dto,
        string tipo,
        CancellationToken cancellationToken = default);

    Task<bool> EliminarAsync(
        int idResponsable,
        string tipo,
        CancellationToken cancellationToken = default);
}