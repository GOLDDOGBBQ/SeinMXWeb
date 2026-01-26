using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using SEINMX.Clases.Generales;
using SEINMX.Context.Database;
using SEINMX.Models.Inventario;
using CotizacionDetalle = SEINMX.Context.Database.CotizacionDetalle;

namespace SEINMX.Context;

public partial class AppClassContext : DbContext
{
    public AppClassContext(DbContextOptions<AppClassContext> options)
        : base(options)
    {
    }

    public virtual DbSet<SpCotizacionDetalleNuevoResult> SpCotizacionDetalleNuevoResults { get; set; }
    public virtual DbSet<SpCotizacionNuevoResult> SpCotizacionNuevoResults { get; set; }
    public virtual DbSet<SpGenericResult> SpGenericResults { get; set; }
    public virtual DbSet<CotizacionOrdenDetalleViewModel> CotizacionOrdenDetalleViewModels { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("VCHAVEZ");
        // registrar tipo keyless (sin clave) y sin mapear a view/tabla
        modelBuilder.Entity<SpCotizacionDetalleNuevoResult>(eb =>
        {
            eb.HasNoKey();
            eb.ToView(null); // evita mapearlo a una vista/tabla
            eb.Property(p => p.IdCotizacionDetalle).HasColumnName("IdCotizacionDetalle");
            eb.Property(p => p.IdCotizacion).HasColumnName("IdCotizacion");
            eb.Property(p => p.IdError).HasColumnName("IdError");
            eb.Property(p => p.MensajeError).HasColumnName("MensajeError");
            eb.Property(p => p.MensajeErrorDev).HasColumnName("MensajeErrorDev");
        });

        modelBuilder.Entity<SpCotizacionNuevoResult>(eb =>
        {
            eb.HasNoKey();
            eb.ToView(null); // evita mapearlo a una vista/tabla
            eb.Property(p => p.IdCotizacion).HasColumnName("IdCotizacion");
            eb.Property(p => p.IdError).HasColumnName("IdError");
            eb.Property(p => p.MensajeError).HasColumnName("MensajeError");
            eb.Property(p => p.MensajeErrorDev).HasColumnName("MensajeErrorDev");
        });

        modelBuilder.Entity<SpGenericResult>(eb =>
        {
            eb.HasNoKey();
            eb.ToView(null);
            eb.Property(p => p.Id).HasColumnName("Id");
            eb.Property(p => p.IdError).HasColumnName("IdError");
            eb.Property(p => p.MensajeError).HasColumnName("MensajeError");
            eb.Property(p => p.MensajeErrorDev).HasColumnName("MensajeErrorDev");
        });

        modelBuilder.Entity<CotizacionOrdenDetalleViewModel>(entity =>
        {
            entity.HasNoKey();
            entity.ToView(null);

            entity.Property(e => e.IdCotizacionDetalle);
            entity.Property(e => e.IdCotizacion);

            entity.Property(e => e.Codigo)
                .HasMaxLength(50);

            entity.Property(e => e.CodigoProveedor)
                .HasMaxLength(50);

            entity.Property(e => e.Descripcion)
                .HasMaxLength(500);

            entity.Property(e => e.IdOrdenCompraDetalle);
            entity.Property(e => e.IdOrdenCompra);

            entity.Property(e => e.CantidadCotizada)
                .HasColumnType("decimal(18,4)");

            entity.Property(e => e.Cantidad)
                .HasColumnType("decimal(18,4)");

            entity.Property(e => e.CantidadDisponible)
                .HasColumnType("decimal(18,4)");

            entity.Property(e => e.PrecioListaMXN)
                .HasColumnType("decimal(18,2)");

            entity.Property(e => e.PrecioProveedor)
                .HasColumnType("decimal(18,2)");

            entity.Property(e => e.PorcentajeProveedor)
                .HasColumnType("decimal(18,4)");

            entity.Property(e => e.Total)
                .HasColumnType("decimal(18,2)");
        });



        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
