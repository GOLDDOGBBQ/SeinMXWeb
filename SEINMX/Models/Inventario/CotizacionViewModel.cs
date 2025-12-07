using Microsoft.AspNetCore.Mvc.Rendering;
using SEINMX.Context.Database;

namespace SEINMX.Models.Inventario;

public class CotizacionViewModel
{
    public VsCotizacion? Cotizacion { get; set; }
    public List<Context.Database.CotizacionDetalle> Detalles { get; set; } = new();

    // Listas para combos (llenadas vía AJAX)
    public List<SelectListItem> Usuarios { get; set; } = new();



}
