using System;
using System.Collections.Generic;

namespace SEINMX.Data;

public partial class Catalogo
{
    public int IdCatalogo { get; set; }

    public int? IdCatalogoMtro { get; set; }

    public string? Modulo { get; set; }

    public string? Nemonico { get; set; }

    public string Descripcion { get; set; } = null!;

    public string? Description { get; set; }

    public int? Valor { get; set; }

    public int? Valor2 { get; set; }

    public string CreadoPor { get; set; } = null!;

    public DateTime FchReg { get; set; }

    public string UsrReg { get; set; } = null!;

    public string? ModificadoPor { get; set; }

    public DateTime? FchAct { get; set; }

    public string? UsrAct { get; set; }

    public bool Eliminado { get; set; }

    public virtual Catalogo? IdCatalogoMtroNavigation { get; set; }

    public virtual ICollection<Catalogo> InverseIdCatalogoMtroNavigation { get; set; } = new List<Catalogo>();
}
