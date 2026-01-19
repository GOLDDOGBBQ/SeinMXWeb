using System;
using System.Collections.Generic;

namespace SEINMX.Context.Database;

public partial class OrdenCompraDetalle
{
    public int IdOrdenCompraDetalle { get; set; }

    public int IdOrdenCompra { get; set; }

    public int IdCotizacionDetalle { get; set; }

    public decimal PrecioProveedor { get; set; }

    public decimal Cantidad { get; set; }

    public decimal Total { get; set; }

    public string CreadoPor { get; set; } = null!;

    public DateTime FchReg { get; set; }

    public string UsrReg { get; set; } = null!;

    public string? ModificadoPor { get; set; }

    public DateTime? FchAct { get; set; }

    public string? UsrAct { get; set; }

    public virtual CotizacionDetalle IdCotizacionDetalleNavigation { get; set; } = null!;

    public virtual OrdenCompra IdOrdenCompraNavigation { get; set; } = null!;
}
