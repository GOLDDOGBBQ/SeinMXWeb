using SEINMX.Context.Database;

namespace SEINMX.Models.Inventario;

public record CotizacionPdfModel(
    string Cotizacion,
    int IdCotizacion,
    DateOnly Fecha,
    decimal? TipoCambio,
    decimal? Tarifa,
    decimal? PorcentajeIVA,
    decimal? Descuento,
    string? UsuarioResponsable,
    int? IdCliente,
    int? IdClienteContacto,
    int? IdClienteRazonSolcial,
    string? Observaciones,
    decimal? SubTotal,
    decimal? Iva,
    decimal? Total,
    decimal? DescuentoTotal,
    bool?  EsIncluirEnvio,
    List<VsCotizacionDetalle> Detalles,

    string?  Cliente,
    string?  NombreContacto,
    string?  Telefono
);

