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

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("VCHAVEZ");
        // registrar tipo keyless (sin clave) y sin mapear a view/tabla
        modelBuilder.Entity<SpCotizacionDetalleNuevoResult>(eb =>
        {
            eb.HasNoKey();
            eb.ToView(null); // evita mapearlo a una vista/tabla
            eb.Property(p => p.IdCotizacionDetalle).HasColumnName("IdCotizacionDetalle");
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
            eb.ToView(null); // evita mapearlo a una vista/tabla
            eb.Property(p => p.Id).HasColumnName("Id");
            eb.Property(p => p.IdError).HasColumnName("IdError");
            eb.Property(p => p.MensajeError).HasColumnName("MensajeError");
            eb.Property(p => p.MensajeErrorDev).HasColumnName("MensajeErrorDev");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
