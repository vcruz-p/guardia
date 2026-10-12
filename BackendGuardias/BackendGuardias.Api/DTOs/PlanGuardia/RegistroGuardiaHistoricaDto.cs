using System;

namespace BackendGuardias.Api.DTOs.PlanGuardia
{
    public sealed class RegistroGuardiaHistoricaDto
    {
        public int IdPersona { get; set; }

        public DateTime FechaGuardia { get; set; }
    }
}