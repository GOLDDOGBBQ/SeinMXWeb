namespace SEINMX.Models.Inventario;

public record CotizacionPdfModel(
    int IdCotizacion,
    DateOnly? Fecha,
    decimal? TipoCambio,
    decimal? Tarifa,
    decimal? PorcentajeIVA,
    decimal? Descuento,
    string? UsuarioResponsable,
    int? IdCliente,
    int? IdClienteContacto,
    int? IdClienteRazonSolcial,
    int Status,
    string? Observaciones,
    decimal? SubTotal,
    decimal? Iva,
    decimal? Total,
    List<CotizacionDetalle> Detalles
);

