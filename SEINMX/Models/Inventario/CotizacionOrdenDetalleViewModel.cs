namespace SEINMX.Models.Inventario;

using System.ComponentModel.DataAnnotations;

public class CotizacionOrdenDetalleViewModel
{
    [Display(Name = "ID Detalle")]
    public int IdCotizacionDetalle { get; set; }

    [Display(Name = "Cotización")]
    public int IdCotizacion { get; set; }

    [Display(Name = "Código")]
    public string Codigo { get; set; }

    [Display(Name = "Código Proveedor")]
    public string? CodigoProveedor { get; set; }

    [Display(Name = "Descripción")]
    public string Descripcion { get; set; }

    [Display(Name = "ID OC Detalle")]
    public int? IdOrdenCompraDetalle { get; set; }

    [Display(Name = "Orden de Compra")]
    public int? IdOrdenCompra { get; set; }

    [Display(Name = "Cantidad Cotizada")]
    [DisplayFormat(DataFormatString = "{0:N2}", ApplyFormatInEditMode = false)]
    public decimal CantidadCotizada { get; set; }

    [Display(Name = "Cantidad OC")]
    [DisplayFormat(DataFormatString = "{0:N2}", ApplyFormatInEditMode = false)]
    public decimal Cantidad { get; set; }

    [Display(Name = "Cantidad Disponible")]
    [DisplayFormat(DataFormatString = "{0:N2}", ApplyFormatInEditMode = false)]
    public decimal CantidadDisponible { get; set; }

    [Display(Name = "Precio Lista (MXN)")]
    [DataType(DataType.Currency)]
    [DisplayFormat(DataFormatString = "{0:C2}", ApplyFormatInEditMode = false)]
    public decimal PrecioListaMXN { get; set; }

    [Display(Name = "Precio Proveedor (MXN)")]
    [DataType(DataType.Currency)]
    [DisplayFormat(DataFormatString = "{0:C2}", ApplyFormatInEditMode = false)]
    public decimal PrecioProveedor { get; set; }

    [Display(Name = "Precio Sein (MXN)")]
    [DataType(DataType.Currency)]
    [DisplayFormat(DataFormatString = "{0:C2}", ApplyFormatInEditMode = false)]
    public decimal PrecioSein{ get; set; }


    [Display(Name = "% Proveedor")]
    [DisplayFormat(DataFormatString = "{0:N2}", ApplyFormatInEditMode = false)]
    public decimal PorcentajeProveedor { get; set; }

    [Display(Name = "% Proveedor Ganancia")]
    [DisplayFormat(DataFormatString = "{0:N2}", ApplyFormatInEditMode = false)]
    public decimal PorcentajeProveedorGanancia { get; set; }



    [Display(Name = "Total (MXN)")]
    [DataType(DataType.Currency)]
    [DisplayFormat(DataFormatString = "{0:C2}", ApplyFormatInEditMode = false)]
    public decimal Total { get; set; }



}