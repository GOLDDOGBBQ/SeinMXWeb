using System;
using System.Collections.Generic;

namespace SEINMX.Context.Database;

public partial class OrdenCompra
{
    public int IdOrdenCompra { get; set; }

    public int IdCotizacion { get; set; }

    public int IdProveedor { get; set; }

    public short? Status { get; set; }

    public DateOnly Fecha { get; set; }

    public decimal TipoCambio { get; set; }

    public decimal PorcentajeProveedor { get; set; }

    public decimal PorcentajeProveedorGanancia { get; set; }

    public decimal PorcentajeIva { get; set; }

    public string CondicionPago { get; set; } = null!;

    public string Observaciones { get; set; } = null!;

    public string CreadoPor { get; set; } = null!;

    public DateTime FchReg { get; set; }

    public string UsrReg { get; set; } = null!;

    public string? ModificadoPor { get; set; }

    public DateTime? FchAct { get; set; }

    public string? UsrAct { get; set; }

    public virtual Cotizacion IdCotizacionNavigation { get; set; } = null!;

    public virtual Proveedor IdProveedorNavigation { get; set; } = null!;

    public virtual ICollection<OrdenCompraDetalle> OrdenCompraDetalles { get; set; } = new List<OrdenCompraDetalle>();
}
