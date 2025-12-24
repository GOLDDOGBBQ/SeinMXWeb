using Microsoft.AspNetCore.Mvc.Rendering;
using SEINMX.Context.Database;
using SEINMX.Models.Inventario;

namespace SEINMX.Models.Directorio;


using System.ComponentModel.DataAnnotations;

public class ProveedorBuscadorViewModel
{
    [Display(Name = "# Proveedor")]
    public int? IdProveedor { get; set; }
    public string? Nombre { get; set; }
    public IEnumerable<SEINMX.Context.Database.Proveedor> Proveedors { get; set; }

}


public class ProveedorViewModel
    {
        public int? IdProveedor { get; set; }

        [Required(ErrorMessage = "El nombre es obligatorio")]
        [StringLength(250, MinimumLength = 1, ErrorMessage = "El nombre no puede exceder 250 caracteres ni estar vacio")]
        [Display(Name = "Nombre")]
        public string Nombre { get; set; } = string.Empty;

        [DataType(DataType.MultilineText)]
        [StringLength(600, ErrorMessage = "La dirección no puede exceder 600 caracteres")]
        [Display(Name = "Dirección")]
        public string? Direccion { get; set; } = string.Empty;

        [StringLength(600, ErrorMessage = "Las observaciones no pueden exceder 600 caracteres")]
        [DataType(DataType.MultilineText)]
        [Display(Name = "Observaciones")]
        public string? Observaciones { get; set; } = string.Empty;

        [Required(ErrorMessage = "La tarifa es obligatoria")]
        [Range(0, double.MaxValue, ErrorMessage = "La tarifa debe ser mayor o igual a 0")]
        [Display(Name = "Tarifa")]
        public decimal Tarifa { get; set; }


        [Range(0, double.MaxValue, ErrorMessage = "La tarifa de ganancia debe ser mayor o igual a 0")]
        [Display(Name = "Tarifa de Ganancia")]
        public decimal TarifaGanancia { get; set; }



    }



