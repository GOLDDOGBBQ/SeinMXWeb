namespace SEINMX.Models.Inventario;

public class SpCotizacionDetalleNuevoResult : RequestGenericStoreProcedure
{
    public int? IdCotizacionDetalle { get; set; }

}

public class SpCotizacionNuevoResult : RequestGenericStoreProcedure
{
    public int? IdCotizacion { get; set; }
}
