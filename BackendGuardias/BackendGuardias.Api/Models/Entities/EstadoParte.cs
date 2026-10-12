using System;
using System.Collections.Generic;

namespace BackendGuardias.Api.Models.Entities;

public partial class EstadoParte
{
    public int IdEstado { get; set; }

    public string Codigo { get; set; } = null!;

    public string Nombre { get; set; } = null!;

    public string? Descripcion { get; set; }

    public string? Color { get; set; }

    public string? Icono { get; set; }

    public bool? Activo { get; set; }

    public int? IdPersonaCrea { get; set; }

    public int? IdPersonaActualiza { get; set; }

    public DateTime FechaHoraCreado { get; set; }

    public DateTime FechaHoraActualizado { get; set; }

    public virtual ICollection<PlanGuardia> PlanGuardia { get; set; } = new List<PlanGuardia>();
}
