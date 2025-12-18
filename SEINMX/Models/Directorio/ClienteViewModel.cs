using Microsoft.AspNetCore.Mvc.Rendering;
using SEINMX.Context.Database;
using SEINMX.Models.Inventario;

namespace SEINMX.Models.Directorio;


using System.ComponentModel.DataAnnotations;

public class ClienteBuscadorViewModel
{
    [Display(Name = "# Cliente")]
    public int? IdCliente { get; set; }
    [Display(Name = "Tipo")]
    public int? IdTipo { get; set; }
    public string? Nombre { get; set; }
    public string? Codigo { get; set; }


    public IEnumerable<SEINMX.Context.Database.Cliente> Clientes { get; set; }

    public List<SelectListItem> GetComboTipoCliente()
    {
        return CombosFijos.GetComboTipoCliente(IdTipo, true);
    }
}


public class ClienteViewModel
    {
        public int? IdCliente { get; set; }

        [Required(ErrorMessage = "El nombre es obligatorio")]
        [StringLength(250, ErrorMessage = "El nombre no puede exceder 250 caracteres")]
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

        [Required(ErrorMessage = "La tarifa es obligatoria")]
        [Range(0, double.MaxValue, ErrorMessage = "La tarifa debe ser mayor o igual a 0")]
        [Display(Name = "Tarifa")]
        public decimal Tarifa { get; set; }

        [Required(ErrorMessage = "El tipo de cliente es obligatorio")]
        [Range(1, 2, ErrorMessage = "Seleccione un tipo válido")]
        [Display(Name = "Tipo de Cliente")]
        public int IdTipo { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "La tarifa de ganancia debe ser mayor o igual a 0")]
        [Display(Name = "Tarifa de Ganancia")]
        public decimal TarifaGanancia { get; set; }

        public virtual ICollection<ClienteContactoViewModel> ClienteContactos { get; set; } = new List<ClienteContactoViewModel>();

        public virtual ICollection<ClienteRazonSolcialViewModel> ClienteRazonSolcials { get; set; } = new List<ClienteRazonSolcialViewModel>();


        public List<SelectListItem> GetComboTipoCliente()
        {
            return CombosFijos.GetComboTipoCliente(IdTipo, false);
        }

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
    public bool? EsPublicoGeneral { get; set; }
    public string? Observaciones { get; set; }
}