using Microsoft.AspNetCore.Mvc.Rendering;
using SEINMX.Models.Inventario;

namespace SEINMX.Models.Directorio;


using System.ComponentModel.DataAnnotations;

public class ClienteBuscadorViewModel
{
    [Display(Name = "# Cliente")]
    public int? IdCliente { get; set; }
    [Display(Name = "Perfil")]
    public int? IdPerfil { get; set; }
    public string? Nombre { get; set; }
    public string? Codigo { get; set; }


    public IEnumerable<Context.Database.VsCliente> Clientes { get; set; }

}


public class ClienteViewModel
    {
        public int? IdCliente { get; set; }

        [Required(ErrorMessage = "El nombre es obligatorio")]
        [StringLength(250, MinimumLength = 1, ErrorMessage = "El nombre no puede exceder 250 caracteres ni estar vacio")]
        [Display(Name = "Nombre")]
        public string Nombre { get; set; } = string.Empty;

        [DataType(DataType.MultilineText)]
        [StringLength(600, ErrorMessage = "La dirección no puede exceder 600 caracteres")]
        [Display(Name = "Dirección")]
        public string? Direccion { get; set; } = string.Empty;

        [StringLength(600, ErrorMessage = "Las observaciones no pueden exceder 600 caracteres")]
        [DataType(DataType.MultilineText)]
        [Display(Name = "Observaciones")]
        public string? Observaciones { get; set; } = string.Empty;

        [Required(ErrorMessage = "El Perfil de cliente es obligatorio")]
        [Display(Name = "Perfil del Cliente")]
        public int IdPerfil { get; set; }

        public virtual ICollection<ClienteContactoViewModel> ClienteContactos { get; set; } = new List<ClienteContactoViewModel>();

        public virtual ICollection<ClienteRazonSolcialViewModel> ClienteRazonSolcials { get; set; } = new List<ClienteRazonSolcialViewModel>();
        public bool esCotizable { get; set; } = false;


    }

public class ClienteContactoViewModel
{
    [Display(Name = "# Contacto")]
    public int? IdClienteContacto { get; set; }
    public string Nombre { get; set; }
    public string? Telefono { get; set; }
    public string? Correo { get; set; }
}

public class ClienteRazonSolcialViewModel
{
    [Display(Name = "# Razon Social")]
    public int? IdClienteRazonSolcial { get; set; }
    public string RFC { get; set; }
    public string RazonSocial { get; set; }
    public string? Domicilio { get; set; }
    public string? CodigoPostal { get; set; }
    public bool? EsPublicoGeneral { get; set; }
    public string? Observaciones { get; set; }
}