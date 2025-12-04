using Microsoft.AspNetCore.Mvc.Rendering;
using SEINMX.Context.Database;

namespace SEINMX.Models.Inventario;

public class CotizacionViewModel
{
    public VsCotizacion? Cotizacion { get; set; }
    public List<CotizacionDetalle> Detalles { get; set; } = new();

    // Combos asincrónicos
    public int? IdUsuarioResponsable { get; set; }

    // Listas para combos (llenadas vía AJAX)
    public List<SelectListItem> Usuarios { get; set; } = new();

    // Para agregar productos desde el panel inferior
    public string? BuscarProducto { get; set; }
}
