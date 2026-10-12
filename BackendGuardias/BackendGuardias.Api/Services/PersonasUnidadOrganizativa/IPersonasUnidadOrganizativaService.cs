using BackendGuardias.Api.DTOs.Comun;
using BackendGuardias.Api.DTOs.PersonasUnidadOrganizativa;
using BackendGuardias.Api.Models.Views;
namespace BackendGuardias.Api.Services.PersonasUnidadOrganizativa;

public interface IPersonasUnidadOrganizativaService
{
    Task<ResultadoConsultaDto<PersonaUnidadSubordinacionView>> ObtenerSubordinadasAsync(
            int idUnidadOrganizativa,
            string? nombre,
            string? nombreUnidadOrganizativa,
            bool incluirPersonasDeLaUnidad,
            CancellationToken cancellationToken = default);

    Task<ResultadoPersonasGrupoGuardiaDto> ObtenerPersonasPorUnidadYGrupoAsync(
            int idUnidadOrganizativa,
            int idGrupoGuardia,
            string? nombreAsignadas = null,
            string? nombreUnidadOrganizativaAsignadas = null,
            string? nombreSinAsignar = null,
            string? nombreUnidadOrganizativaSinAsignar = null,
            bool incluirPersonasDeLaUnidad = false,
            CancellationToken cancellationToken = default);

            Task<IReadOnlyList<UnidadOrganizativaArbolDto>> ObtenerArbolCompletoAsync(
        int? idUnidadOrganizativa,
        string? nombrePersona,
        string? nombreUnidadOrganizativa,
        bool incluirPersonas = true,
        CancellationToken cancellationToken = default);


}