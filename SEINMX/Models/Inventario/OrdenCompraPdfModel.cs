using SEINMX.Context.Database;

namespace SEINMX.Models.Inventario;

public record OrdenCompraPdfModel(
    int IdOrdenCompra,
    DateOnly Fecha,
    decimal? TipoCambio,
    string? CondicionPago,
    string? Proveedor,
    string? ProveedorRfc,
    string? ProveedorRazonSocial,
    string? StatusDesc,
    string? Observaciones,
    decimal? SubTotal,
    decimal? Iva,
    decimal? Total,
    List<VsOrdenCompraDetalle> Detalles
);

