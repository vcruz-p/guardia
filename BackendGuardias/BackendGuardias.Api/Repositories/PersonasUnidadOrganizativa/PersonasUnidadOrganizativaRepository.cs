
using BackendGuardias.Api.Data;
using BackendGuardias.Api.Models.Views;
using Microsoft.EntityFrameworkCore;

namespace BackendGuardias.Api.Repositories.PersonasUnidadOrganizativa;

public sealed class PersonasUnidadOrganizativaRepository
    : IPersonasUnidadOrganizativaRepository
{
    private readonly GuardiasDbContext _context;

    public PersonasUnidadOrganizativaRepository(
        GuardiasDbContext context)
    {
        _context = context;
    }

    // ============================================================
    // PERSONAS DE UNA UNIDAD Y SUS UNIDADES SUBORDINADAS
    // ============================================================

    public async Task<IReadOnlyList<PersonaUnidadSubordinacionView>>
        ObtenerSubordinadasAsync(
            int idUnidadOrganizativa,
            string? nombre,
            string? nombreUnidadOrganizativa,
            bool incluirPersonasDeLaUnidad,
            CancellationToken cancellationToken = default)
    {
        if (idUnidadOrganizativa <= 0)
        {
            return Array.Empty<PersonaUnidadSubordinacionView>();
        }

        // Obtener todas las unidades desde la vista del árbol.
        // Esto incluye unidades sin personas asignadas.
        var unidades = await _context.VistaArbolUnidadesOrganizativas
            .AsNoTracking()
            .Select(u => new UnidadOrganizativaArbolFila
            {
                IdUnidadOrganizativa = u.IdUnidadOrganizativa,
                Nombre = u.Nombre,
                Descripcion = u.Descripcion,
                IdsUnidadesSuperiores = u.IdsUnidadesSuperiores
            })
            .ToListAsync(cancellationToken);

        var unidadesPorId = unidades
            .GroupBy(u => u.IdUnidadOrganizativa)
            .ToDictionary(g => g.Key, g => g.First());

        if (!unidadesPorId.ContainsKey(idUnidadOrganizativa))
        {
            return Array.Empty<PersonaUnidadSubordinacionView>();
        }

        // Localizar la unidad solicitada y todos sus descendientes.
        // Funciona tanto con relaciones directas como con listas
        // que contienen todos los ancestros.
        var idsUnidadesIncluidas = new HashSet<int>
        {
            idUnidadOrganizativa
        };

        bool huboCambios;

        do
        {
            huboCambios = false;

            foreach (var unidad in unidades)
            {
                if (idsUnidadesIncluidas.Contains(
                        unidad.IdUnidadOrganizativa))
                {
                    continue;
                }

                var superiores = ObtenerIdsSuperiores(
                    unidad.IdsUnidadesSuperiores);

                if (superiores.Any(idsUnidadesIncluidas.Contains))
                {
                    idsUnidadesIncluidas.Add(
                        unidad.IdUnidadOrganizativa);

                    huboCambios = true;
                }
            }
        }
        while (huboCambios);

        // Excluir a la propia unidad cuando se solicita solamente
        // el personal de sus unidades subordinadas.
        if (!incluirPersonasDeLaUnidad)
        {
            idsUnidadesIncluidas.Remove(idUnidadOrganizativa);
        }

        if (idsUnidadesIncluidas.Count == 0)
        {
            return Array.Empty<PersonaUnidadSubordinacionView>();
        }

        // Consultar las personas en la vista de personas.
        var consulta = _context.PersonasUnidadOrganizativa
            .AsNoTracking()
            .Where(p =>
                p.IdUnidadOrganizativa.HasValue &&
                idsUnidadesIncluidas.Contains(
                    p.IdUnidadOrganizativa.Value));

        if (!string.IsNullOrWhiteSpace(nombre))
        {
            var filtroNombre = nombre.Trim();

            consulta = consulta.Where(p =>
                p.NombreCompleto != null &&
                p.NombreCompleto.Contains(filtroNombre));
        }

        if (!string.IsNullOrWhiteSpace(nombreUnidadOrganizativa))
        {
            var filtroUnidad = nombreUnidadOrganizativa.Trim();

            consulta = consulta.Where(p =>
                p.UnidadOrganizativaNombre != null &&
                p.UnidadOrganizativaNombre.Contains(filtroUnidad));
        }

        return await consulta
            .OrderBy(p => p.UnidadOrganizativaNombre)
            .ThenBy(p => p.Apellidos)
            .ThenBy(p => p.Nombre)
            .ToListAsync(cancellationToken);
    }

    // ============================================================
    // PERSONAS ASIGNADAS A UN GRUPO DE GUARDIA
    // ============================================================

    public async Task<IReadOnlyList<int>>
        ObtenerIdsPersonasAsignadasAlGrupoAsync(
            int idGrupoGuardia,
            CancellationToken cancellationToken = default)
    {
        var hoy = DateTime.Today;

        var ids = await _context.Database
            .SqlQuery<int>($"""
                SELECT DISTINCT gp.id_persona AS Value
                FROM guardia.grupo_guardia_persona AS gp
                WHERE gp.id_grupo_guardia = {idGrupoGuardia}
                  AND gp.activo = 1
                  AND gp.fecha_inicio <= {hoy}
                  AND (
                      gp.fecha_fin IS NULL
                      OR gp.fecha_fin >= {hoy}
                  )
                """)
            .ToListAsync(cancellationToken);

        return ids;
    }

    // ============================================================
    // OBTENER TODAS LAS UNIDADES PARA CONSTRUIR EL ÁRBOL
    // ============================================================

    public async Task<IReadOnlyList<UnidadOrganizativaArbolFila>>
        ObtenerUnidadesParaArbolAsync(
            int? idUnidadOrganizativa,
            CancellationToken cancellationToken = default)
    {
        // Esta vista incluye todas las unidades, también las vacías.
        // No utilizar aquí la vista de personas.
        var unidades = await _context.VistaArbolUnidadesOrganizativas
            .AsNoTracking()
            .OrderBy(u => u.Nombre)
            .Select(u => new UnidadOrganizativaArbolFila
            {
                IdUnidadOrganizativa = u.IdUnidadOrganizativa,
                Nombre = u.Nombre,
                Descripcion = u.Descripcion,
                IdsUnidadesSuperiores = u.IdsUnidadesSuperiores
            })
            .ToListAsync(cancellationToken);

        if (unidades.Count == 0)
        {
            return Array.Empty<UnidadOrganizativaArbolFila>();
        }

        var unidadesPorId = unidades
            .GroupBy(u => u.IdUnidadOrganizativa)
            .ToDictionary(g => g.Key, g => g.First());

        // Convertir la lista de superiores de cada unidad en una lista
        // de identificadores existentes.
        var superioresPorId = unidadesPorId.ToDictionary(
            pareja => pareja.Key,
            pareja => ObtenerIdsSuperiores(
                    pareja.Value.IdsUnidadesSuperiores)
                .Where(id =>
                    id != pareja.Key &&
                    unidadesPorId.ContainsKey(id))
                .Distinct()
                .ToList());

        // Inferir el padre directo descartando los ancestros indirectos.
        foreach (var unidad in unidades)
        {
            var superiores = superioresPorId[
                unidad.IdUnidadOrganizativa];

            var padresDirectos = superiores
                .Where(candidato =>
                    !superiores.Any(otro =>
                        otro != candidato &&
                        superioresPorId.TryGetValue(
                            otro,
                            out var ancestrosDelOtro) &&
                        ancestrosDelOtro.Contains(candidato)))
                .OrderBy(id => id)
                .ToList();

            // La estructura DTO existente admite un único padre.
            // Si hay varios superiores directos, seleccionar uno
            // de forma determinista.
            unidad.IdUnidadOrganizativaPadre =
                padresDirectos.Count > 0
                    ? padresDirectos[0]
                    : null;
        }

        // Sin unidad inicial, devolver todas las unidades.
        if (!idUnidadOrganizativa.HasValue)
        {
            return unidades;
        }

        var idRaiz = idUnidadOrganizativa.Value;

        if (!unidadesPorId.ContainsKey(idRaiz))
        {
            return Array.Empty<UnidadOrganizativaArbolFila>();
        }

        // Incluir la raíz y recorrer todos sus niveles descendientes.
        var idsIncluidos = new HashSet<int> { idRaiz };

        bool huboCambios;

        do
        {
            huboCambios = false;

            foreach (var unidad in unidades)
            {
                if (idsIncluidos.Contains(
                        unidad.IdUnidadOrganizativa))
                {
                    continue;
                }

                var superiores = superioresPorId[
                    unidad.IdUnidadOrganizativa];

                if (superiores.Any(idsIncluidos.Contains))
                {
                    idsIncluidos.Add(
                        unidad.IdUnidadOrganizativa);

                    huboCambios = true;
                }
            }
        }
        while (huboCambios);

        var resultado = unidades
            .Where(u =>
                idsIncluidos.Contains(u.IdUnidadOrganizativa))
            .ToList();

        // En un árbol parcial, la unidad solicitada debe ser la raíz.
        foreach (var unidad in resultado)
        {
            if (unidad.IdUnidadOrganizativa == idRaiz)
            {
                unidad.IdUnidadOrganizativaPadre = null;
            }
            else if (unidad.IdUnidadOrganizativaPadre.HasValue &&
                     !idsIncluidos.Contains(
                         unidad.IdUnidadOrganizativaPadre.Value))
            {
                unidad.IdUnidadOrganizativaPadre = null;
            }
        }

        return resultado;
    }

    // ============================================================
    // PERSONAS DE UN CONJUNTO DE UNIDADES
    // ============================================================

    public async Task<IReadOnlyList<VistaPersonasUnidadesSubordinacion>>
        ObtenerPersonasDeUnidadesAsync(
            IReadOnlyCollection<int> idsUnidades,
            string? nombrePersona,
            string? nombreUnidadOrganizativa,
            CancellationToken cancellationToken = default)
    {
        if (idsUnidades is null || idsUnidades.Count == 0)
        {
            return Array.Empty<VistaPersonasUnidadesSubordinacion>();
        }

        var ids = idsUnidades.Distinct().ToArray();

        var consulta = _context.VistaPersonasUnidadesSubordinacion
            .AsNoTracking()
            .Where(p =>
                p.IdUnidadOrganizativa.HasValue &&
                ids.Contains(p.IdUnidadOrganizativa.Value));

        if (!string.IsNullOrWhiteSpace(nombrePersona))
        {
            var filtroNombre = nombrePersona.Trim();

            consulta = consulta.Where(p =>
                (p.Nombre != null &&
                 p.Nombre.Contains(filtroNombre)) ||
                (p.Apellidos != null &&
                 p.Apellidos.Contains(filtroNombre)) ||
                (p.NombreCompleto != null &&
                 p.NombreCompleto.Contains(filtroNombre)));
        }

        if (!string.IsNullOrWhiteSpace(nombreUnidadOrganizativa))
        {
            var filtroUnidad = nombreUnidadOrganizativa.Trim();

            consulta = consulta.Where(p =>
                p.UnidadOrganizativaNombre != null &&
                p.UnidadOrganizativaNombre.Contains(filtroUnidad));
        }

        return await consulta
            .OrderBy(p => p.UnidadOrganizativaNombre)
            .ThenBy(p => p.Apellidos)
            .ThenBy(p => p.Nombre)
            .ToListAsync(cancellationToken);
    }

    // ============================================================
    // AUXILIAR: CONVERTIR IDS DE SUPERIORES EN ENTEROS
    // ============================================================

    private static List<int> ObtenerIdsSuperiores(string? ids)
    {
        if (string.IsNullOrWhiteSpace(ids))
        {
            return [];
        }

        return ids
            .Split(
                ',',
                StringSplitOptions.RemoveEmptyEntries |
                StringSplitOptions.TrimEntries)
            .Select(id => int.TryParse(id, out var valor)
                ? (int?)valor
                : null)
            .Where(id => id.HasValue)
            .Select(id => id!.Value)
            .Distinct()
            .ToList();
    }
}
