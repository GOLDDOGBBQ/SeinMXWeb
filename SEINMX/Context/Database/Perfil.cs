using System;
using System.Collections.Generic;

namespace SEINMX.Context.Database;

public partial class Perfil
{
    public int IdPerfil { get; set; }

    public string Perfil1 { get; set; } = null!;

    public string Identificador { get; set; } = null!;

    public decimal Tarifa { get; set; }

    public string CreadoPor { get; set; } = null!;

    public DateTime FchReg { get; set; }

    public string UsrReg { get; set; } = null!;

    public string? ModificadoPor { get; set; }

    public DateTime? FchAct { get; set; }

    public string? UsrAct { get; set; }

    public bool Eliminado { get; set; }

    public virtual ICollection<Cliente> Clientes { get; set; } = new List<Cliente>();

    public virtual ICollection<Cotizacion> Cotizacions { get; set; } = new List<Cotizacion>();
}
