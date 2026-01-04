using System;
using System.Collections.Generic;

namespace SEINMX.Context.Database;

public partial class TipoCambioDof
{
    public int IdTipoCambioDof { get; set; }

    public decimal? TipoCambio { get; set; }

    public DateOnly? Fecha { get; set; }

    public string CreadoPor { get; set; } = null!;

    public DateTime FchReg { get; set; }

    public string UsrReg { get; set; } = null!;

    public string? ModificadoPor { get; set; }

    public DateTime? FchAct { get; set; }

    public string? UsrAct { get; set; }
}
