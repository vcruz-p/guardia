using BackendGuardias.Api.DTOs.Comun;
using BackendGuardias.Api.DTOs.PersonasUnidadOrganizativa;
using BackendGuardias.Api.Models.Views;
using BackendGuardias.Api.Repositories.PersonasUnidadOrganizativa;

namespace BackendGuardias.Api.Services.PersonasUnidadOrganizativa;

public sealed class PersonasUnidadOrganizativaService
    : IPersonasUnidadOrganizativaService
{
    private readonly IPersonasUnidadOrganizativaRepository _repository;

    public PersonasUnidadOrganizativaService(
        IPersonasUnidadOrganizativaRepository repository)
    {
        _repository = repository;
    }

    public async Task<ResultadoConsultaDto<PersonaUnidadSubordinacionView>>
        ObtenerSubordinadasAsync(
            int idUnidadOrganizativa,
            string? nombre,
            string? nombreUnidadOrganizativa,
            bool incluirPersonasDeLaUnidad,
            CancellationToken cancellationToken = default)
    {
        ValidarIdUnidadOrganizativa(idUnidadOrganizativa);

        var personas = await _repository.ObtenerSubordinadasAsync(
            idUnidadOrganizativa,
            nombre,
            nombreUnidadOrganizativa,
            incluirPersonasDeLaUnidad,
            cancellationToken);

        return new ResultadoConsultaDto<PersonaUnidadSubordinacionView>
        {
            Total = personas.Count,
            Data = personas
        };
    }

    public async Task<ResultadoPersonasGrupoGuardiaDto>
        ObtenerPersonasPorUnidadYGrupoAsync(
            int idUnidadOrganizativa,
            int idGrupoGuardia,
            string? nombreAsignadas = null,
            string? nombreUnidadOrganizativaAsignadas = null,
            string? nombreSinAsignar = null,
            string? nombreUnidadOrganizativaSinAsignar = null,
            bool incluirPersonasDeLaUnidad = false,
            CancellationToken cancellationToken = default)
    {
        ValidarIdUnidadOrganizativa(idUnidadOrganizativa);
        ValidarIdGrupoGuardia(idGrupoGuardia);

        var personas = await _repository.ObtenerSubordinadasAsync(
            idUnidadOrganizativa,
            null,
            null,
            incluirPersonasDeLaUnidad,
            cancellationToken);

        var idsAsignados =
            await _repository.ObtenerIdsPersonasAsignadasAlGrupoAsync(
                idGrupoGuardia,
                cancellationToken);

        var conjuntoIdsAsignados = idsAsignados.ToHashSet();

        var personasAsignadas = personas
            .Where(p => conjuntoIdsAsignados.Contains(p.IdPersona));

        var personasSinAsignar = personas
            .Where(p => !conjuntoIdsAsignados.Contains(p.IdPersona));

        var asignadas = FiltrarPersonas(
            personasAsignadas,
            nombreAsignadas,
            nombreUnidadOrganizativaAsignadas);

        var sinAsignar = FiltrarPersonas(
            personasSinAsignar,
            nombreSinAsignar,
            nombreUnidadOrganizativaSinAsignar);

        return new ResultadoPersonasGrupoGuardiaDto
        {
            Asignadas = new ResultadoConsultaDto<PersonaUnidadSubordinacionView>
            {
                Total = asignadas.Count,
                Data = asignadas
            },
            SinAsignar = new ResultadoConsultaDto<PersonaUnidadSubordinacionView>
            {
                Total = sinAsignar.Count,
                Data = sinAsignar
            }
        };
    }

    private static List<PersonaUnidadSubordinacionView> FiltrarPersonas(
        IEnumerable<PersonaUnidadSubordinacionView> personas,
        string? nombre,
        string? nombreUnidadOrganizativa)
    {
        var nombreFiltro = nombre?.Trim();
        var unidadFiltro = nombreUnidadOrganizativa?.Trim();

        return personas
            .Where(p =>
                (
                    string.IsNullOrWhiteSpace(nombreFiltro)
                    || (p.NombreCompleto?.Contains(
                        nombreFiltro,
                        StringComparison.OrdinalIgnoreCase) ?? false)
                )
                &&
                (
                    string.IsNullOrWhiteSpace(unidadFiltro)
                    || (p.UnidadOrganizativaNombre?.Contains(
                        unidadFiltro,
                        StringComparison.OrdinalIgnoreCase) ?? false)
                ))
            .ToList();
    }

    private static void ValidarIdUnidadOrganizativa(
        int idUnidadOrganizativa)
    {
        if (idUnidadOrganizativa <= 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(idUnidadOrganizativa),
                "El ID de unidad organizativa debe ser mayor que cero.");
        }
    }

    private static void ValidarIdGrupoGuardia(int idGrupoGuardia)
    {
        if (idGrupoGuardia <= 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(idGrupoGuardia),
                "El ID del grupo de guardia debe ser mayor que cero.");
        }
    }

    // ============================================================
    // ÁRBOL COMPLETO DE UNIDADES ORGANIZATIVAS
    // ============================================================


public async Task<IReadOnlyList<UnidadOrganizativaArbolDto>>
    ObtenerArbolCompletoAsync(
        int? idUnidadOrganizativa,
        string? nombrePersona,
        string? nombreUnidadOrganizativa,
        bool incluirPersonas,
        CancellationToken cancellationToken = default)
{
    if (idUnidadOrganizativa.HasValue &&
        idUnidadOrganizativa.Value <= 0)
    {
        throw new ArgumentException(
            "El ID de unidad organizativa debe ser mayor que cero.",
            nameof(idUnidadOrganizativa));
    }

    var unidadesConsultadas =
        await _repository.ObtenerUnidadesParaArbolAsync(
            idUnidadOrganizativa,
            cancellationToken);

    if (unidadesConsultadas.Count == 0)
    {
        return Array.Empty<UnidadOrganizativaArbolDto>();
    }

    var unidades = unidadesConsultadas
        .GroupBy(u => u.IdUnidadOrganizativa)
        .Select(g => g.First())
        .ToList();

    var unidadesPorId = unidades.ToDictionary(
        u => u.IdUnidadOrganizativa);

    var filtroPersona = string.IsNullOrWhiteSpace(nombrePersona)
        ? null
        : nombrePersona.Trim();

    var filtroUnidad = string.IsNullOrWhiteSpace(nombreUnidadOrganizativa)
        ? null
        : nombreUnidadOrganizativa.Trim();

    var aplicarFiltros =
        filtroPersona is not null ||
        filtroUnidad is not null;

    IReadOnlyList<VistaPersonasUnidadesSubordinacion> personas =
        Array.Empty<VistaPersonasUnidadesSubordinacion>();

    // Consultar personas si se van a incluir en la respuesta
    // o si son necesarias para filtrar las unidades.
    if (incluirPersonas || filtroPersona is not null)
    {
        personas = await _repository.ObtenerPersonasDeUnidadesAsync(
            unidades.Select(u => u.IdUnidadOrganizativa).ToArray(),
            filtroPersona,
            null,
            cancellationToken);
    }

    var idsObjetivo = unidadesPorId.Keys.ToHashSet();

    // Filtrar por el nombre de la unidad organizativa.
    if (filtroUnidad is not null)
    {
        var idsPorUnidad = unidades
            .Where(u => u.Nombre.Contains(
                filtroUnidad,
                StringComparison.OrdinalIgnoreCase))
            .Select(u => u.IdUnidadOrganizativa)
            .ToHashSet();

        idsObjetivo.IntersectWith(idsPorUnidad);
    }

    // Filtrar por unidades que contienen personas coincidentes.
    if (filtroPersona is not null)
    {
        var idsPorPersona = personas
            .Where(p => p.IdUnidadOrganizativa.HasValue)
            .Select(p => p.IdUnidadOrganizativa!.Value)
            .ToHashSet();

        idsObjetivo.IntersectWith(idsPorPersona);
    }

    if (idsObjetivo.Count == 0)
    {
        return Array.Empty<UnidadOrganizativaArbolDto>();
    }

    // Cuando hay filtros, conservar los ancestros de las unidades
    // coincidentes para mantener el contexto del árbol.
    var idsSeleccionados = aplicarFiltros
        ? AgregarAncestrosDelArbol(idsObjetivo, unidadesPorId)
        : unidadesPorId.Keys.ToHashSet();

    var nodos = unidades
        .Where(u => idsSeleccionados.Contains(u.IdUnidadOrganizativa))
        .ToDictionary(
            u => u.IdUnidadOrganizativa,
            u => new UnidadOrganizativaArbolDto
            {
                IdUnidadOrganizativa = u.IdUnidadOrganizativa,
                IdUnidadOrganizativaPadre =
                    u.IdUnidadOrganizativaPadre,
                Nombre = u.Nombre,
                Descripcion = u.Descripcion,
                IdCategoria = u.IdCategoria,
                CategoriaNombre = u.CategoriaNombre,
                CategoriaDescripcion = u.CategoriaDescripcion,
                UnidadesSuperiores = u.UnidadesSuperiores,
                UnidadesSuperioresDescripcion = u.UnidadesSuperioresDescripcion,

                Personas = incluirPersonas
                    ? personas
                        .Where(p =>
                            p.IdUnidadOrganizativa ==
                            u.IdUnidadOrganizativa)
                        .ToList()
                    : null,

                UnidadesHijas =
                    new List<UnidadOrganizativaArbolDto>()
            });

    var raices = new List<UnidadOrganizativaArbolDto>();

    // Enlazar cada nodo con su padre.
    // Como las hijas también contienen sus propias hijas,
    // se conservan todos los niveles del árbol.
    foreach (var unidad in unidades
                 .Where(u =>
                     idsSeleccionados.Contains(u.IdUnidadOrganizativa))
                 .OrderBy(u => u.Nombre))
    {
        var nodo = nodos[unidad.IdUnidadOrganizativa];

        var idPadre = unidad.IdUnidadOrganizativaPadre;

        if (idPadre.HasValue &&
            idPadre.Value != unidad.IdUnidadOrganizativa &&
            nodos.TryGetValue(idPadre.Value, out var padre))
        {
            padre.UnidadesHijas.Add(nodo);
        }
        else
        {
            nodo.IdUnidadOrganizativaPadre = null;
            raices.Add(nodo);
        }
    }

    OrdenarArbolRecursivamente(raices);

    // Si se solicitó una unidad concreta, devolverla como raíz.
    if (idUnidadOrganizativa.HasValue)
    {
        if (nodos.TryGetValue(
                idUnidadOrganizativa.Value,
                out var raizSolicitada))
        {
            raizSolicitada.IdUnidadOrganizativaPadre = null;
            return new[] { raizSolicitada };
        }

        return Array.Empty<UnidadOrganizativaArbolDto>();
    }

    return raices;
}

private static HashSet<int> AgregarAncestrosDelArbol(
    HashSet<int> idsObjetivo,
    IReadOnlyDictionary<int, UnidadOrganizativaArbolFila> unidadesPorId)
{
    var seleccionados = new HashSet<int>();

    foreach (var idObjetivo in idsObjetivo)
    {
        var idActual = idObjetivo;
        var visitados = new HashSet<int>();

        while (unidadesPorId.TryGetValue(idActual, out var unidad) &&
               visitados.Add(idActual))
        {
            seleccionados.Add(idActual);

            if (!unidad.IdUnidadOrganizativaPadre.HasValue)
            {
                break;
            }

            idActual = unidad.IdUnidadOrganizativaPadre.Value;
        }
    }

    return seleccionados;
}

private static void OrdenarArbolRecursivamente(
    List<UnidadOrganizativaArbolDto> unidades)
{
    unidades.Sort((a, b) =>
        string.Compare(
            a.Nombre,
            b.Nombre,
            StringComparison.OrdinalIgnoreCase));

    foreach (var unidad in unidades)
    {
        OrdenarArbolRecursivamente(unidad.UnidadesHijas);
    }
}


}