using System;
using System.Collections.Generic;

namespace SEINMX.Context.Database;

public partial class VsOrdenCompra
{
    public int IdOrdenCompra { get; set; }

    public int IdCotizacion { get; set; }

    public int IdProveedor { get; set; }

    public string? Proveedor { get; set; }

    public string? ProveedorRfc { get; set; }

    public string? ProveedorRazonSocial { get; set; }

    public short? Status { get; set; }

    public decimal TipoCambio { get; set; }

    public decimal PorcentajeProveedor { get; set; }

    public decimal PorcentajeProveedorGanancia { get; set; }

    public string StatusDesc { get; set; } = null!;

    public DateOnly Fecha { get; set; }

    public decimal? FactorIva { get; set; }

    public string CondicionPago { get; set; } = null!;

    public string Observaciones { get; set; } = null!;

    public string? Cotizacion { get; set; }

    public string? Cliente { get; set; }

    public decimal SubTotal { get; set; }

    public decimal? Iva { get; set; }

    public decimal? Total { get; set; }
}
