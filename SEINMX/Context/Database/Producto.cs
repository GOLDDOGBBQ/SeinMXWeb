using System;
using System.Collections.Generic;

namespace SEINMX.Context.Database;

public partial class Producto
{
    public int IdProducto { get; set; }

    public int IdProveedor { get; set; }

    public int? IdMoneda { get; set; }

    public string Codigo { get; set; } = null!;

    public string Descripcion { get; set; } = null!;

    public decimal PrecioLista { get; set; }

    public string ClaveUnidadSat { get; set; } = null!;

    public string Observaciones { get; set; } = null!;

    public string CreadoPor { get; set; } = null!;

    public DateTime FchReg { get; set; }

    public string UsrReg { get; set; } = null!;

    public string? ModificadoPor { get; set; }

    public DateTime? FchAct { get; set; }

    public string? UsrAct { get; set; }

    public bool Eliminado { get; set; }

    public virtual ICollection<CotizacionDetalle> CotizacionDetalles { get; set; } = new List<CotizacionDetalle>();

    public virtual Proveedor IdProveedorNavigation { get; set; } = null!;
}
