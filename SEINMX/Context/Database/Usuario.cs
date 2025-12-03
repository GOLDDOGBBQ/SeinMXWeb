using System;
using System.Collections.Generic;

namespace SEINMX.Context.Database;

public partial class Usuario
{
    public int IdUsuario { get; set; }

    public string? Nombre { get; set; }

    public string Usuario1 { get; set; } = null!;

    public string? Email { get; set; }

    public string? Telefono { get; set; }

    public bool Admin { get; set; }

    public string PasswordHash { get; set; } = null!;

    public bool CambiarPassword { get; set; }

    public DateTime? UltimoAcceso { get; set; }

    public string CreadoPor { get; set; } = null!;

    public DateTime FchReg { get; set; }

    public string UsrReg { get; set; } = null!;

    public string? ModificadoPor { get; set; }

    public DateTime? FchAct { get; set; }

    public string? UsrAct { get; set; }

    public bool Eliminado { get; set; }

    public virtual ICollection<Cotizacion> Cotizacions { get; set; } = new List<Cotizacion>();
}
