using BackendGuardias.Api.DTOs.Comun;
using BackendGuardias.Api.Models.Views;

namespace BackendGuardias.Api.DTOs.PersonasUnidadOrganizativa;

public class ResultadoPersonasGrupoGuardiaDto
{
    public ResultadoConsultaDto<PersonaUnidadSubordinacionView> Asignadas
    {
        get;
        set;
    } = new();

    public ResultadoConsultaDto<PersonaUnidadSubordinacionView> SinAsignar
    {
        get;
        set;
    } = new();
}