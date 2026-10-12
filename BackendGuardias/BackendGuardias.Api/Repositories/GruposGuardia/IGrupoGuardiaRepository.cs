using BackendGuardias.Api.Models.Entities;
using BackendGuardias.Api.Models.Views;

namespace BackendGuardias.Api.Repositories.GruposGuardia;

public interface IGrupoGuardiaRepository
{
    Task<IReadOnlyList<VistaGruposGuardia>> ObtenerVistasAsync(
        bool incluirInactivos,
        CancellationToken cancellationToken);

    Task<VistaGruposGuardia?> ObtenerVistaPorIdAsync(
        int id,
        CancellationToken cancellationToken);

    Task<IReadOnlyList<GrupoGuardia>> ObtenerTodosAsync(
        bool incluirInactivos,
        CancellationToken cancellationToken);

    Task<GrupoGuardia?> ObtenerPorIdAsync(
        int id,
        CancellationToken cancellationToken);

    Task<GrupoGuardia?> ObtenerParaActualizarAsync(
        int id,
        CancellationToken cancellationToken);

    Task<bool> ExisteNombreOInicialesAsync(
        string nombre,
        string iniciales,
        int? idExcluir,
        CancellationToken cancellationToken);

    void Agregar(GrupoGuardia grupo);

    Task GuardarCambiosAsync(CancellationToken cancellationToken);
}
