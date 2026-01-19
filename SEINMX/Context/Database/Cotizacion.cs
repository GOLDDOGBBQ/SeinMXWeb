using System;
using System.Collections.Generic;

namespace SEINMX.Context.Database;

public partial class Cotizacion
{
    public int IdCotizacion { get; set; }

    public DateOnly? Fecha { get; set; }

    public int? Status { get; set; }

    public string? UsuarioResponsable { get; set; }

    public int IdCliente { get; set; }

    public int? IdClienteContacto { get; set; }

    public int? IdClienteRazonSolcial { get; set; }

    public decimal TipoCambio { get; set; }

    public decimal Descuento { get; set; }

    public string Observaciones { get; set; } = null!;

    public decimal PorcentajeIva { get; set; }

    public decimal Tarifa { get; set; }

    public string CreadoPor { get; set; } = null!;

    public DateTime FchReg { get; set; }

    public string UsrReg { get; set; } = null!;

    public string? ModificadoPor { get; set; }

    public DateTime? FchAct { get; set; }

    public string? UsrAct { get; set; }

    public bool EsIncluirEnvio { get; set; }

    public int? IdPerfil { get; set; }

    public virtual ICollection<CotizacionDetalle> CotizacionDetalles { get; set; } = new List<CotizacionDetalle>();

    public virtual ClienteContacto? IdClienteContactoNavigation { get; set; }

    public virtual Cliente IdClienteNavigation { get; set; } = null!;

    public virtual ClienteRazonSolcial? IdClienteRazonSolcialNavigation { get; set; }

    public virtual Perfil? IdPerfilNavigation { get; set; }

    public virtual ICollection<OrdenCompra> OrdenCompras { get; set; } = new List<OrdenCompra>();

    public virtual Usuario? UsuarioResponsableNavigation { get; set; }
}
