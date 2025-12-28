using System;
using System.Collections.Generic;

namespace SEINMX.Context.Database;

public partial class VsCotizacion
{
    public string Cotizacion { get; set; } = null!;

    public int IdCotizacion { get; set; }

    public DateOnly? Fecha { get; set; }

    public int? Status { get; set; }

    public string StatusDesc { get; set; } = null!;

    public string? UsuarioResponsable { get; set; }

    public string? Responsable { get; set; }

    public int IdCliente { get; set; }

    public int? IdClienteContacto { get; set; }

    public int? IdClienteRazonSolcial { get; set; }

    public bool EsIncluirEnvio { get; set; }

    public int? IdPerfil { get; set; }

    public string? Perfil { get; set; }

    public string? Identificador { get; set; }

    public decimal Tarifa { get; set; }

    public decimal TipoCambio { get; set; }

    public decimal PorcentajeIva { get; set; }

    public decimal Descuento { get; set; }

    public string Observaciones { get; set; } = null!;

    public string? Cliente { get; set; }

    public string? NombreContacto { get; set; }

    public string? Telefono { get; set; }

    public string? Correo { get; set; }

    public string? Rfc { get; set; }

    public string? RazonSocial { get; set; }

    public decimal SubTotal { get; set; }

    public decimal? FactorDescuento { get; set; }

    public decimal? FactorIva { get; set; }

    public decimal? Iva { get; set; }

    public decimal? Total { get; set; }

    public decimal? DescuentoTotal { get; set; }
}
