using BackendGuardias.Api.Data;
using BackendGuardias.Api.Models.Views;
using Microsoft.EntityFrameworkCore;

namespace BackendGuardias.Api.Repositories.PersonasUnidadOrganizativa;

public interface IPersonasUnidadOrganizativaRepository
{
    Task<IReadOnlyList<PersonaUnidadSubordinacionView>> ObtenerSubordinadasAsync(
            int idUnidadOrganizativa,
            string? nombre,
            string? nombreUnidadOrganizativa,
            bool incluirPersonasDeLaUnidad,
            CancellationToken cancellationToken = default);

    Task<IReadOnlyList<int>> ObtenerIdsPersonasAsignadasAlGrupoAsync(
            int idGrupoGuardia,
            CancellationToken cancellationToken = default);

            Task<IReadOnlyList<UnidadOrganizativaArbolFila>> ObtenerUnidadesParaArbolAsync(
        int? idUnidadOrganizativa,
        CancellationToken cancellationToken = default);

Task<IReadOnlyList<VistaPersonasUnidadesSubordinacion>> ObtenerPersonasDeUnidadesAsync(
        IReadOnlyCollection<int> idsUnidades,
        string? nombrePersona,
        string? nombreUnidadOrganizativa,
        CancellationToken cancellationToken = default);
}