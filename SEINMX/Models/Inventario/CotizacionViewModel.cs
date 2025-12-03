using Microsoft.AspNetCore.Mvc.Rendering;
using SEINMX.Context.Database;

namespace SEINMX.Models.Inventario;

public class CotizacionViewModel
{
    public VsCotizacion? Cotizacion { get; set; }
    public List<CotizacionDetalle> Detalles { get; set; } = new();

    // Combos asincrónicos
    public int? IdUsuarioResponsable { get; set; }
    public int? IdCliente { get; set; }
    public int? IdClienteeContacto { get; set; }
    public int? IdClienteRazonSolcial { get; set; }

    // Listas para combos (llenadas vía AJAX)
    public List<SelectListItem> Usuarios { get; set; } = new();
    public List<SelectListItem> Clientes { get; set; } = new();
    public List<SelectListItem> Contactos { get; set; } = new();
    public List<SelectListItem> RazonesSociales { get; set; } = new();

    // Para agregar productos desde el panel inferior
    public string? BuscarProducto { get; set; }
}
