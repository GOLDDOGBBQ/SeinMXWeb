using System;
using System.Collections.Generic;

namespace SEINMX.Context.Database;

public partial class ProductosImport
{
    public string? Item { get; set; }

    public string? Descripción { get; set; }

    public decimal? PrecioDeLista { get; set; }

    public string? MonedaRaiz { get; set; }
}
