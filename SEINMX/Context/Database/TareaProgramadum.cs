using System;
using System.Collections.Generic;

namespace SEINMX.Context.Database;

public partial class TareaProgramadum
{
    public int IdTareaProgramada { get; set; }

    public string Nombre { get; set; } = null!;

    public string ExpresionCron { get; set; } = null!;

    public DateTimeOffset? UltimaEjecucion { get; set; }

    public bool Activa { get; set; }

    public string ZonaHoraria { get; set; } = null!;

    public string CreadoPor { get; set; } = null!;

    public DateTime FchReg { get; set; }

    public string UsrReg { get; set; } = null!;

    public string? ModificadoPor { get; set; }

    public DateTime? FchAct { get; set; }

    public string? UsrAct { get; set; }

    public string Descripcion { get; set; } = null!;

    public DateTimeOffset? FchActivacion { get; set; }
}
