using System;
using System.Collections.Generic;

namespace SEINMX.Context.Database;

public partial class Cliente
{
    public int IdCliente { get; set; }

    public string Nombre { get; set; } = null!;

    public string Direccion { get; set; } = null!;

    public string Observaciones { get; set; } = null!;

    public decimal Tarifa { get; set; }

    public int IdTipo { get; set; }

    public string CreadoPor { get; set; } = null!;

    public DateTime FchReg { get; set; }

    public string UsrReg { get; set; } = null!;

    public string? ModificadoPor { get; set; }

    public DateTime? FchAct { get; set; }

    public string? UsrAct { get; set; }

    public bool Eliminado { get; set; }

    public virtual ICollection<ClienteContacto> ClienteContactos { get; set; } = new List<ClienteContacto>();

    public virtual ICollection<ClienteRazonSolcial> ClienteRazonSolcials { get; set; } = new List<ClienteRazonSolcial>();

    public virtual ICollection<Cotizacion> Cotizacions { get; set; } = new List<Cotizacion>();

    public virtual ICollection<Producto> Productos { get; set; } = new List<Producto>();
}
