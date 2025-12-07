using System;
using System.Collections.Generic;

namespace SEINMX.Context.Database;

public partial class VsCotizacionDetalle
{
    public int IdCotizacionDetalle { get; set; }

    public int IdCotizacion { get; set; }

    public int IdProducto { get; set; }

    public string Codigo { get; set; } = null!;

    public string Descripcion { get; set; } = null!;

    public decimal PrecioLista { get; set; }

    public string ClaveUnidadSat { get; set; } = null!;

    public int? IdMoneda { get; set; }

    public decimal PorcentajeProveedor { get; set; }

    public decimal PorcentajeProveedorGanancia { get; set; }

    public decimal PrecioListaMxn { get; set; }

    public decimal PrecioProveedor { get; set; }

    public decimal GananciaProveedor { get; set; }

    public decimal PrecioSein { get; set; }

    public decimal PrecioCliente { get; set; }

    public decimal Cantidad { get; set; }

    public decimal Total { get; set; }

    public string Observaciones { get; set; } = null!;
}
