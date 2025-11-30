using System;
using System.Collections.Generic;

namespace SEINMX.Context.Database;

public partial class ClienteRazonSolcial
{
    public int IdClienteRazonSolcial { get; set; }

    public int IdCliente { get; set; }

    public string Rfc { get; set; } = null!;

    public string RazonSocial { get; set; } = null!;

    public bool EsPublicoGeneral { get; set; }

    public string Domicilio { get; set; } = null!;

    public string Observaciones { get; set; } = null!;

    public string CreadoPor { get; set; } = null!;

    public DateTime FchReg { get; set; }

    public string UsrReg { get; set; } = null!;

    public string? ModificadoPor { get; set; }

    public DateTime? FchAct { get; set; }

    public string? UsrAct { get; set; }

    public bool Eliminado { get; set; }

    public virtual Cliente IdClienteNavigation { get; set; } = null!;
}
