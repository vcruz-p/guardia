using System;
using System.Collections.Generic;

namespace BackendGuardias.Api.Models.Entities;

public partial class GrupoGuardiaPersona
{
    public int IdGrupoGuardiaPersona { get; set; }

    public int IdGrupoGuardia { get; set; }

    public int IdPersona { get; set; }

    public DateTime FechaInicio { get; set; }

    public DateTime? FechaFin { get; set; }

    public bool? Activo { get; set; }

    public string? Observacion { get; set; }

    public int? IdPersonaCrea { get; set; }

    public int? IdPersonaActualiza { get; set; }

    public DateTime FechaHoraCreado { get; set; }

    public DateTime FechaHoraActualizado { get; set; }

    public virtual GrupoGuardia IdGrupoGuardiaNavigation { get; set; } = null!;
}
