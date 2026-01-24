namespace SEINMX.Models.Inventario;

public class OrdenCompraDetalleRequest
{
    public int? IdOrdenCompraDetalle { get; set; }
    public int IdOrdenCompra { get; set; }

    public int IdCotizacion { get; set; }
    public int IdCotizacionDetalle { get; set; }
    public decimal Cantidad { get; set; }
}