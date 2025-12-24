using System;
using System.Collections.Generic;

namespace SEINMX.Context.Database;

public partial class VsCliente
{
    public int IdCliente { get; set; }

    public string Nombre { get; set; } = null!;

    public bool Eliminado { get; set; }

    public int IdPerfil { get; set; }

    public string Direccion { get; set; } = null!;

    public string Observaciones { get; set; } = null!;

    public string CreadoPor { get; set; } = null!;

    public DateTime FchReg { get; set; }

    public string UsrReg { get; set; } = null!;

    public string? ModificadoPor { get; set; }

    public DateTime? FchAct { get; set; }

    public string? UsrAct { get; set; }

    public string? Perfil { get; set; }

    public decimal? Tarifa { get; set; }

    public string? Identificador { get; set; }
}
