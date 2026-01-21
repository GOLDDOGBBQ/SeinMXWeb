using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace SEINMX.Models.Inventario;

public class OrdenCompraViewModel
{
    [Required(ErrorMessage = "El # Orden Compra es requerido.")]
    public int IdOrdenCompra { get; set; }

    [Display(Name = "# Cotización")]
    public string? Cotizacion { get; set; }


    // -------------------------
    // DATOS GENERALES DEL FORM
    // -------------------------

    [Required(ErrorMessage = "La fecha es obligatoria.")]
    [DataType(DataType.Date)]
    [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
    public DateOnly? Fecha { get; set; }



    [Display(Name = "Cliente")]
    public string? Cliente { get; set; }

    [Display(Name = "Proveedor")]
    public string? Proveedor { get; set; }

    // -------------------------
    // STATUS
    // -------------------------

    [Required(ErrorMessage = "Debe seleccionar el estatus.")]
    [Range(1, 5, ErrorMessage = "Status inválido.")]
    public int Status { get; set; }

    // -------------------------
    // OBSERVACIONES
    // -------------------------
    [DataType(DataType.MultilineText)]
    [MaxLength(600, ErrorMessage = "Las observaciones no pueden exceder 600 caracteres.")]
    public string? Observaciones { get; set; }

    // -------------------------
    // Datos de consulta en la vista
    // -------------------------
    [Display(Name = "Sub totalSub total")]
    public decimal? SubTotal { get; set; }

    [Display(Name = "IVA")]
    public decimal? Iva { get; set; }
    public decimal? Total { get; set; }

    public List<SelectListItem> GetComboStatus()
    {
        return CombosFijos.GetComboStatusOrdenCompra(Status, false);
    }
}

public class OrdenCompraBuscadorViewModel
{
    [Display(Name = "# Orden Compra")]
    public int? IdOrdenCompra { get; set; }
    [Display(Name = "# Cotización")]
    public int? IdCotizacion { get; set; }
    public string? Cliente { get; set; }
    public int? Status { get; set; }
    public IEnumerable<SEINMX.Context.Database.VsOrdenCompra> Ordenes { get; set; }


    public List<SelectListItem> GetComboStatus()
    {
        return CombosFijos.GetComboStatusOrdenCompra(Status, true);
    }
}


public class OrdenCompraNuevaViewModel
{

    [Display(Name = "Cotización")]
    [Required(ErrorMessage = "Debe seleccionar una Cotización.")]
    public int IdCotizacion { get; set; }

    [Display(Name = "Proveedor")]
    [Required(ErrorMessage = "Debe seleccionar un Proveedor.")]
    public int IdProveedor { get; set; }

    public int Status { get; set; }
    public DateOnly? Fecha { get; set; }


}

