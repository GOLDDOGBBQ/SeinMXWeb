namespace SEINMX.Models.Inventario;

public partial class CotizacionDetalle
{
    public int IdCotizacionDetalle { get; set; }
    public int IdCotizacion { get; set; }
    public int IdProducto { get; set; }
    public string Producto { get; set; } = "";
    public decimal Cantidad { get; set; }
    public decimal Precio { get; set; }
    public decimal Importe { get; set; }

    public string CreadoPor { get; set; } = "";
    public string ModificadoPor { get; set; } = "";
    public string UsrReg { get; set; } = "";
    public string UsrAct { get; set; } = "";
    public DateTime? FchReg { get; set; }
    public DateTime? FchAct { get; set; }
}