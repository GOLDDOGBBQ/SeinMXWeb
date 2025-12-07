namespace SEINMX.Models.Inventario;

public class AgregarProductoRequest
{
    public int? IdCotizacionDetalle { get; set; }
    public int IdCotizacion { get; set; }
    public int IdProducto { get; set; }
    public decimal Cantidad { get; set; }
    public string Observaciones { get; set; }
}
