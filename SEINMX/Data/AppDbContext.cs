using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using SEINMX.Models.Database;

namespace SEINMX.Data;

public partial class AppDbContext : DbContext
{
    public AppDbContext()
    {
    }

    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Usuario> Usuarios { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        => optionsBuilder.UseSqlServer("name=ConnectionUpadate");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("VCHAVEZ");

        modelBuilder.Entity<Usuario>(entity =>
        {
            entity.HasKey(e => e.IdUsuario).HasName("PK__Usuario__5B65BF97CC0AA100");

            entity.ToTable("Usuario", "SSM");

            entity.HasIndex(e => e.Usuario1, "UQ__Usuario__E3237CF76E1D9A0A").IsUnique();

            entity.Property(e => e.CreadoPor).HasMaxLength(100);
            entity.Property(e => e.Email)
                .HasMaxLength(50)
                .HasDefaultValue("")
                .HasColumnName("EMail");
            entity.Property(e => e.FchAct).HasColumnType("datetime");
            entity.Property(e => e.FchReg).HasColumnType("datetime");
            entity.Property(e => e.ModificadoPor).HasMaxLength(100);
            entity.Property(e => e.Nombre)
                .HasMaxLength(50)
                .HasDefaultValue("");
            entity.Property(e => e.Password)
                .HasMaxLength(256)
                .HasDefaultValue("");
            entity.Property(e => e.Telefono)
                .HasMaxLength(50)
                .HasDefaultValue("");
            entity.Property(e => e.UltimoAcceso).HasPrecision(3);
            entity.Property(e => e.UsrAct).HasMaxLength(50);
            entity.Property(e => e.UsrReg).HasMaxLength(50);
            entity.Property(e => e.Usuario1)
                .HasMaxLength(25)
                .HasDefaultValue("")
                .HasColumnName("Usuario");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
