using Microsoft.AspNetCore.Mvc.Rendering;

namespace SEINMX.Models.Inventario;


using System.ComponentModel.DataAnnotations;

public class ProductoBuscadorViewModel
{
    public int? IdProducto { get; set; }
    public string? Descripcion { get; set; }
    public string? Codigo { get; set; }

    public IEnumerable<SEINMX.Context.Database.VsProducto> Productos { get; set; }

}


public class ProductoViewModel
    {
        public int? IdProducto { get; set; }

        // ===========================
        // Campos obligatorios
        // ===========================
        [Required(ErrorMessage = "El proveedor es obligatorio.")]
        [Display(Name = "Proveedor")]
        public int IdProveedor { get; set; }

        [Required(ErrorMessage = "El código es obligatorio.")]
        [StringLength(50, ErrorMessage = "El código no puede exceder 50 caracteres.")]
        public string Codigo { get; set; } = string.Empty;

        [StringLength(50, ErrorMessage = "El código no puede exceder 50 caracteres.")]
        [Display(Name = "Codigo Proveedor")]
        public string? CodigoProveedor { get; set; } = string.Empty;

        [Required(ErrorMessage = "La descripción es obligatoria.")]
        [StringLength(500, ErrorMessage = "La descripción no puede exceder 500 caracteres.")]
        [Display(Name = "Descripción")]
        public string Descripcion { get; set; } = string.Empty;

        [Required(ErrorMessage = "El precio de lista es obligatorio.")]
        [Range(0.01, 99999999, ErrorMessage = "El precio debe ser mayor que cero.")]
        [Display(Name = "Precio Lista")]
        public decimal PrecioLista { get; set; }

        [Required(ErrorMessage = "La existencia es obligatoria.")]
        [Range(0, 99999999, ErrorMessage = "La existencia no puede ser negativa.")]
        public decimal Existencia { get; set; }


        // ===========================
        // Campos opcionales
        // ===========================
        [Display(Name = "Moneda")]
        public int? IdMoneda { get; set; }

        [StringLength(50, ErrorMessage = "La clave SAT no puede exceder 50 caracteres.")]

        [Display(Name = "Clave Unidad SAT")]
        public string? ClaveUnidadSAT { get; set; } = string.Empty;

        [DataType(DataType.MultilineText)]
        [StringLength(600, ErrorMessage = "Las observaciones no pueden exceder 600 caracteres.")]
        public string? Observaciones { get; set; } = string.Empty;


        // ===========================
        // Auditoría (no se captura en formulario)
        // ===========================
        public string? CreadoPor { get; set; }
        public DateTime? FchReg { get; set; }
        public string? UsrReg { get; set; }

        public string? ModificadoPor { get; set; }
        public DateTime? FchAct { get; set; }
        public string? UsrAct { get; set; }

        public bool Eliminado { get; set; } = false;

        public List<SelectListItem> GetComboMoneda()
        {
            return CombosFijos.GetComboMoneda(IdMoneda, false);
        }

    }