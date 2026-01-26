using SEINMX.Clases.Generales;

namespace SEINMX.Models.Inventario;

public class SpCotizacionDetalleNuevoResult : RequestGenericStoreProcedure
{
    public int? IdCotizacionDetalle { get; set; }
    public int? IdCotizacion { get; set; }

}

public class SpCotizacionNuevoResult : RequestGenericStoreProcedure
{
    public int? IdCotizacion { get; set; }
}

public class SpGenericResult : RequestGenericStoreProcedure
{
    public int? Id { get; set; }
}


