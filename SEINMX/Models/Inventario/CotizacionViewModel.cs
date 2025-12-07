using Microsoft.AspNetCore.Mvc.Rendering;
using SEINMX.Context.Database;

using System.ComponentModel.DataAnnotations;

namespace SEINMX.Models.Inventario;


    public class CotizacionViewModel
    {
        [Required(ErrorMessage = "El IdCotizacion es requerido.")]
        public int IdCotizacion { get; set; }

        // -------------------------
        // DATOS GENERALES DEL FORM
        // -------------------------

        [Required(ErrorMessage = "La fecha es obligatoria.")]
        [DataType(DataType.Date)]
        public DateOnly? Fecha { get; set; }

        [Required(ErrorMessage = "El tipo de cambio es obligatorio.")]
        [Range(0.0001, 999999, ErrorMessage = "El tipo de cambio debe ser mayor a 0.")]
        public decimal? TipoCambio { get; set; }

        [Required(ErrorMessage = "La tarifa es obligatoria.")]
        [Range(0, 100, ErrorMessage = "La tarifa debe estar entre 0 y 100.")]
        public decimal? Tarifa { get; set; }

        [Required(ErrorMessage = "El IVA es obligatorio.")]
        [Range(0, 100, ErrorMessage = "El IVA debe estar entre 0 y 100.")]
        public decimal? PorcentajeIVA { get; set; }

        [Range(0, 999999999, ErrorMessage = "El descuento no puede ser negativo.")]
        public decimal? Descuento { get; set; }

        // -------------------------
        // COMBOS DEL FORM
        // -------------------------

        [Required(ErrorMessage = "Debe seleccionar un usuario responsable.")]
        public string? UsuarioResponsable { get; set; }

        [Required(ErrorMessage = "Debe seleccionar un cliente.")]
        public int? IdCliente { get; set; }

        public int? IdClienteContacto { get; set; }

        public int? IdClienteRazonSolcial { get; set; }

        // -------------------------
        // STATUS
        // -------------------------

        [Required(ErrorMessage = "Debe seleccionar el estatus.")]
        [Range(1, 6, ErrorMessage = "Status inválido.")]
        public int Status { get; set; }

        // -------------------------
        // OBSERVACIONES
        // -------------------------

        [MaxLength(600, ErrorMessage = "Las observaciones no pueden exceder 600 caracteres.")]
        public string? Observaciones { get; set; }

        // -------------------------
        // Datos de consulta en la vista
        // -------------------------
        public decimal? SubTotal { get; set; }
        public decimal? Iva { get; set; }
        public decimal? Total { get; set; }

    }

