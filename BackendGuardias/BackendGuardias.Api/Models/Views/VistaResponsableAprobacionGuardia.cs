using System;

namespace BackendGuardias.Api.Models.Views;

public partial class VistaResponsableAprobacionGuardia
{
    public int IdResponsable { get; set; }
    public int IdUnidadOrganizativa { get; set; }
    public string UnidadOrganizativaNombre { get; set; } = null!;
    public string? UnidadOrganizativaDescripcion { get; set; }
    public int? IdCategoriaUnidadOrganizativa { get; set; }
    public string? CategoriaUnidadOrganizativaNombre { get; set; }
    public string? CategoriaUnidadOrganizativaDescripcion { get; set; }

    public string Tipo { get; set; } = null!;
    public int IdPersona { get; set; }
    public string Nombre { get; set; } = null!;
    public string? Apellidos { get; set; }
    public string? PersonaNombreCompleto { get; set; }
    public string? CarnetIdentidad { get; set; }

    public int? IdSexo { get; set; }
    public string? SexoNombre { get; set; }
    public string? SexoDescripcion { get; set; }
    public string? Sexo { get; set; }
    public int? IdColorPiel { get; set; }
    public string? ColorPielNombre { get; set; }
    public string? ColorPielDescripcion { get; set; }

    public int? IdCargoUnidadOrganizativa { get; set; }
    public int? IdCargo { get; set; }
    public string? CargoNombre { get; set; }
    public string? CargoDescripcion { get; set; }
    public int? IdUnidadOrganizativaPersona { get; set; }
    public string? UnidadOrganizativaPersonaNombre { get; set; }
    public string? UnidadOrganizativaPersonaDescripcion { get; set; }

    public int? IdGrado { get; set; }
    public string? GradoNombre { get; set; }
    public string? GradoDescripcion { get; set; }
    public string? Grado { get; set; }

    public int IdPersonaCrea { get; set; }
    public string? PersonaCreaNombre { get; set; }
    public DateTime FechaHoraCreado { get; set; }
    public DateTime FechaHoraActualizado { get; set; }
}
