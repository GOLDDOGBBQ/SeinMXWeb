using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace SEINMX.Models.Finanzas;

public class MovimientoFinancieroFilterViewModel
{
    [Display(Name = "Fecha desde")]
    public string? FechaDesde { get; set; }

    [Display(Name = "Fecha hasta")]
    public string? FechaHasta { get; set; }

    [Display(Name = "Proveedor")]
    public string? ProveedorNombre { get; set; }

    [Display(Name = "Monto mínimo")]
    public decimal? MontoMin { get; set; }

    [Display(Name = "Monto máximo")]
    public decimal? MontoMax { get; set; }

    [Display(Name = "Pendiente facturar")]
    public bool? PendienteFacturar { get; set; }

    [Display(Name = "Tipo")]
    public byte? Tipo { get; set; }

    public List<MovimientoFinancieroRowViewModel> Items { get; set; } = new();
}

public class MovimientoFinancieroRowViewModel
{
    public int IdMovimientoFinanciero { get; set; }
    public byte Tipo { get; set; }
    public DateOnly Fecha { get; set; }
    public string Descripcion { get; set; } = "";
    public decimal Monto { get; set; }
    public int? IdProveedor { get; set; }
    public string? ProveedorNombre { get; set; }
    public string Factura { get; set; } = "";
    public bool PendienteFacturar { get; set; }
    public int Orden { get; set; }
}

public class MovimientoFinancieroSaveRequest
{
    public int? IdMovimientoFinanciero { get; set; }

    [Range(1, 2, ErrorMessage = "Tipo inválido")]
    public byte Tipo { get; set; }

    [Required(ErrorMessage = "La fecha es requerida")]
    public string? FechaStr { get; set; }

    [Required(ErrorMessage = "La descripción es requerida")]
    [MaxLength(500)]
    public string Descripcion { get; set; } = "";

    [Range(0.01, double.MaxValue, ErrorMessage = "El monto debe ser mayor a 0")]
    public decimal Monto { get; set; }

    public int? IdProveedor { get; set; }

    [MaxLength(100)]
    public string? Factura { get; set; }

    public bool PendienteFacturar { get; set; }
}

public class ReordenarRequest
{
    public List<int> Ids { get; set; } = new();
}