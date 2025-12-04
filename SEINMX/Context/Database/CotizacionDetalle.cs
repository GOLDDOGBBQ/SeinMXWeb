using System;
using System.Collections.Generic;

namespace SEINMX.Context.Database;

public partial class CotizacionDetalle
{
    public int IdCotizacionDetalle { get; set; }

    public int IdCotizacion { get; set; }

    public int IdProducto { get; set; }

    public decimal PorcentajeProveedor { get; set; }

    public decimal PorcentajeProveedorGanancia { get; set; }

    public int? IdMoneda { get; set; }

    public decimal PrecioListaMxn { get; set; }

    public decimal PrecioProveedor { get; set; }

    public decimal GananciaProveedor { get; set; }

    public decimal PrecioSein { get; set; }

    public decimal PrecioCliente { get; set; }

    public decimal Cantidad { get; set; }

    public decimal Total { get; set; }

    public string Observaciones { get; set; } = null!;

    public string CreadoPor { get; set; } = null!;

    public DateTime FchReg { get; set; }

    public string UsrReg { get; set; } = null!;

    public string? ModificadoPor { get; set; }

    public DateTime? FchAct { get; set; }

    public string? UsrAct { get; set; }

    public virtual Cotizacion IdCotizacionNavigation { get; set; } = null!;

    public virtual Producto IdProductoNavigation { get; set; } = null!;
}
