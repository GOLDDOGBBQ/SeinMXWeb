using System;
using System.Collections.Generic;

namespace SEINMX.Context.Database;

public partial class MovimientoFinanciero
{
    public int IdMovimientoFinanciero { get; set; }

    public byte Tipo { get; set; }

    public DateOnly Fecha { get; set; }

    public string Descripcion { get; set; } = null!;

    public decimal Monto { get; set; }

    public int? IdProveedor { get; set; }

    public string Factura { get; set; } = null!;

    public bool PendienteFacturar { get; set; }

    public int Orden { get; set; }

    public string? CreadoPor { get; set; }

    public DateTime FchReg { get; set; }

    public string UsrReg { get; set; } = null!;

    public string? ModificadoPor { get; set; }

    public DateTime? FchAct { get; set; }

    public string? UsrAct { get; set; }

    public bool Eliminado { get; set; }

    public virtual Proveedor? IdProveedorNavigation { get; set; }
}
