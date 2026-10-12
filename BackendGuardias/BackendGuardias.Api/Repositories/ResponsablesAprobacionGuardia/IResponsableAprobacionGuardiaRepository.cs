using BackendGuardias.Api.Models.Entities;
using BackendGuardias.Api.Models.Views;

namespace BackendGuardias.Api.Repositories.ResponsablesAprobacionGuardia;

public interface IResponsableAprobacionGuardiaRepository
{
    Task<IReadOnlyList<VistaResponsableAprobacionGuardia>>
        ObtenerTodosAsync(
            string tipo,
            int? idUnidadOrganizativa,
            int? idPersona,
            string? buscar,
            CancellationToken cancellationToken = default);

    Task<VistaResponsableAprobacionGuardia?>
        ObtenerVistaPorIdAsync(
            int idResponsable,
            string tipo,
            CancellationToken cancellationToken = default);

    Task<ResponsableAprobacionGuardia?>
        ObtenerEntidadPorIdAsync(
            int idResponsable,
            string tipo,
            CancellationToken cancellationToken = default);

    Task<bool> ExistePersonaAsync(
        int idPersona,
        CancellationToken cancellationToken = default);

    Task<bool> ExisteUnidadOrganizativaAsync(
        int idUnidadOrganizativa,
        CancellationToken cancellationToken = default);

    Task<bool> ExisteTipoEnUnidadAsync(
        int idUnidadOrganizativa,
        string tipo,
        int? idResponsableExcluir = null,
        CancellationToken cancellationToken = default);

    Task AgregarAsync(
        ResponsableAprobacionGuardia responsable,
        CancellationToken cancellationToken = default);

    void Eliminar(ResponsableAprobacionGuardia responsable);

    Task<int> GuardarCambiosAsync(
        CancellationToken cancellationToken = default);
}