using System;
using System.Collections.Generic;

namespace SEINMX.Context.Database;

public partial class VsProducto
{
    public int IdProducto { get; set; }

    public int IdProveedor { get; set; }

    public string? Proveedor { get; set; }

    public int? IdMoneda { get; set; }

    public string Moneda { get; set; } = null!;

    public string Codigo { get; set; } = null!;

    public string? CodigoProveedor { get; set; }

    public string Descripcion { get; set; } = null!;

    public decimal PrecioLista { get; set; }

    public string ClaveUnidadSat { get; set; } = null!;

    public string Observaciones { get; set; } = null!;

    public bool Eliminado { get; set; }
}
