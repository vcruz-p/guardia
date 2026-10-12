using System;
using System.Collections.Generic;

namespace BackendGuardias.Api.Models.Views;

public partial class VistaPersonasUnidadesSubordinacion
{
    public int IdPersona { get; set; }

    public string Nombre { get; set; } = null!;

    public string? Apellidos { get; set; }

    public string? NombreCompleto { get; set; }

    public string? CarnetIdentidad { get; set; }

    public int? IdUnidadOrganizativa { get; set; }

    public string? UnidadOrganizativaNombre { get; set; }

    public string? UnidadOrganizativaDescripcion { get; set; }

    public int? IdCargo { get; set; }

    public string? CargoDescripcion { get; set; }

    public int? IdGrado { get; set; }

    public string? GradoDescripcion { get; set; }

    public string? IdsUnidadesSuperiores { get; set; }

    public string? UnidadesSuperiores { get; set; }

    public string? UnidadesSuperioresDescripcion { get; set; }
}
