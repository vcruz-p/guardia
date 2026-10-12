using System;
using System.Collections.Generic;

namespace BackendGuardias.Api.Models.Entities;

public partial class GrupoGuardia
{
    public int IdGrupoGuardia { get; set; }

    public string Nombre { get; set; } = null!;

    public string Iniciales { get; set; } = null!;

    public int? SubordinadoAIdGrupoGuardia { get; set; }

    public string? Color { get; set; }

    public string? Icono { get; set; }

    public string? Observacion { get; set; }

    public bool? Activo { get; set; }

    public int? IdPersonaCrea { get; set; }

    public int? IdPersonaActualiza { get; set; }

    public DateTime FechaHoraCreado { get; set; }

    public DateTime FechaHoraActualizado { get; set; }

    public virtual ICollection<DetallePlanGuardia> DetallePlanGuardia { get; set; } = new List<DetallePlanGuardia>();

    public virtual ICollection<GrupoGuardiaPersona> GrupoGuardiaPersona { get; set; } = new List<GrupoGuardiaPersona>();

    public virtual ICollection<GrupoGuardia> InverseSubordinadoAIdGrupoGuardiaNavigation { get; set; } = new List<GrupoGuardia>();

    public virtual GrupoGuardia? SubordinadoAIdGrupoGuardiaNavigation { get; set; }
}
