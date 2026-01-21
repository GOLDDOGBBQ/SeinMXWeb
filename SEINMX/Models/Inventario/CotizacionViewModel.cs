using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace SEINMX.Models.Inventario;

public class CotizacionViewModel
{
    [Required(ErrorMessage = "El # Cotizacion es requerido.")]
    public int IdCotizacion { get; set; }


    [Display(Name = "# Cotización")]
    public string? Cotizacion { get; set; }

    // -------------------------
    // DATOS GENERALES DEL FORM
    // -------------------------

    [Required(ErrorMessage = "La fecha es obligatoria.")]
    [DataType(DataType.Date)]
    [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
    public DateOnly? Fecha { get; set; }


    [Display(Name = "Tipo Cambio")]
    [Required(ErrorMessage = "El tipo de cambio es obligatorio.")]
    [Range(0.0001, 999999, ErrorMessage = "El tipo de cambio debe ser mayor a 0.")]
    public decimal? TipoCambio { get; set; }

    [Display(Name = "Perfil de la Cotizacion")]
    public string? Perfil { get; set; }

    [Display(Name = "Descuento (%)")]
    [Range(0, 999999999, ErrorMessage = "El descuento no puede ser negativo.")]
    public decimal? Descuento { get; set; }

    // -------------------------
    // COMBOS DEL FORM
    // -------------------------
    [Display(Name = "Usuario Responsable")]
    [Required(ErrorMessage = "Debe seleccionar un usuario responsable.")]
    public string? UsuarioResponsable { get; set; }

    [Display(Name = "Cliente")]
    public int? IdCliente { get; set; }
    public string? Cliente { get; set; }

    [Display(Name = "Contacto")]
    public int? IdClienteContacto { get; set; }

    [Display(Name = "Razón Social")]
    public int? IdClienteRazonSolcial { get; set; }

    // -------------------------
    // STATUS
    // -------------------------

    [Required(ErrorMessage = "Debe seleccionar el estatus.")]
    [Range(1, 6, ErrorMessage = "Status inválido.")]
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

    [Display(Name = "Incluir Envio")]
    public bool EsIncluirEnvio { get; set; }


    public List<SelectListItem> GetComboStatus()
    {
        return CombosFijos.GetComboStatusCotizacion(Status, false);
    }
}

public class CotizacionBuscadorViewModel
{
    [Display(Name = "# Cotización")]
    public int? IdCotizacion { get; set; }
    public string? Cliente { get; set; }
    public int? Status { get; set; }
    public IEnumerable<SEINMX.Context.Database.VsCotizacion> Cotizaciones { get; set; }


    public List<SelectListItem> GetComboStatus()
    {
        return CombosFijos.GetComboStatusCotizacion(Status, true);
    }
}


public class CotizacionNuevaViewModel
{



    [Display(Name = "Cliente")]
    [Required(ErrorMessage = "Debe seleccionar un cliente.")]
    public int IdCliente { get; set; }

    [Display(Name = "Contacto")]
    public int? IdClienteContacto { get; set; }

    [Display(Name = "Razón Social")]
    public int? IdClienteRazonSolcial { get; set; }

}

