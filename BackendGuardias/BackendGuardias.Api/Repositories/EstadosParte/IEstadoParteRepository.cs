using BackendGuardias.Api.Models.Entities;

namespace BackendGuardias.Api.Repositories.EstadosParte;

public interface IEstadoParteRepository
{
    Task<IReadOnlyList<EstadoParte>> ObtenerTodosAsync(
        string? buscar,
        bool? activo,
        CancellationToken cancellationToken = default);

    Task<EstadoParte?> ObtenerPorIdAsync(
        int idEstado,
        CancellationToken cancellationToken = default);

    Task<bool> ExisteCodigoAsync(
        string codigo,
        int? idEstadoExcluir = null,
        CancellationToken cancellationToken = default);

    Task AgregarAsync(
        EstadoParte estado,
        CancellationToken cancellationToken = default);

    Task<int> GuardarCambiosAsync(
        CancellationToken cancellationToken = default);
}