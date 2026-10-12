namespace BackendGuardias.Api.Services.GruposGuardia;

public sealed class GrupoGuardiaDuplicadoException : Exception
{
    public GrupoGuardiaDuplicadoException()
        : base("Ya existe un grupo con ese nombre o iniciales.")
    {
    }
}
