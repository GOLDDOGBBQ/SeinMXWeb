using System;
using System.Collections.Generic;

namespace SEINMX.Context.Database;

public partial class VsOrdenCompraDetalle
{
    public int IdOrdenCompraDetalle { get; set; }

    public int IdOrdenCompra { get; set; }

    public int IdCotizacionDetalle { get; set; }

    public string Codigo { get; set; } = null!;

    public string? CodigoProveedor { get; set; }

    public string Descripcion { get; set; } = null!;

    public decimal PrecioListaMxn { get; set; }

    public decimal PorcentajeProveedor { get; set; }

    public decimal PorcentajeProveedorGanancia { get; set; }

    public decimal PrecioProveedor { get; set; }

    public decimal PrecioSein { get; set; }

    public decimal Cantidad { get; set; }

    public decimal Total { get; set; }
}
