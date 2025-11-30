using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using SEINMX.Context.Database;

namespace SEINMX.Context;

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

    public virtual DbSet<Cliente> Clientes { get; set; }

    public virtual DbSet<ClienteContacto> ClienteContactos { get; set; }

    public virtual DbSet<ClienteRazonSolcial> ClienteRazonSolcials { get; set; }

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
            entity.Property(e => e.PasswordHash)
                .HasMaxLength(60)
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


        modelBuilder.Entity<Cliente>(entity =>
        {
            entity.HasKey(e => e.IdCliente).HasName("PK__Clientes__5EB79C2156870867");

            entity.ToTable("Cliente", "DRO");

            entity.Property(e => e.CreadoPor).HasMaxLength(100);
            entity.Property(e => e.Direccion)
                .HasMaxLength(600)
                .HasDefaultValue("");
            entity.Property(e => e.FchAct).HasColumnType("datetime");
            entity.Property(e => e.FchReg).HasColumnType("datetime");
            entity.Property(e => e.ModificadoPor).HasMaxLength(100);
            entity.Property(e => e.Nombre)
                .HasMaxLength(250)
                .HasDefaultValue("");
            entity.Property(e => e.Observaciones)
                .HasMaxLength(600)
                .HasDefaultValue("");
            entity.Property(e => e.Tarifa).HasColumnType("numeric(18, 4)");
            entity.Property(e => e.UsrAct).HasMaxLength(50);
            entity.Property(e => e.UsrReg).HasMaxLength(50);
        });

        modelBuilder.Entity<ClienteContacto>(entity =>
        {
            entity.HasKey(e => e.IdClienteContacto).HasName("PK__ClienteC__61CE65D3196CC942");

            entity.ToTable("ClienteContacto", "DRO");

            entity.Property(e => e.Correo)
                .HasMaxLength(250)
                .HasDefaultValue("");
            entity.Property(e => e.CreadoPor).HasMaxLength(100);
            entity.Property(e => e.FchAct).HasColumnType("datetime");
            entity.Property(e => e.FchReg).HasColumnType("datetime");
            entity.Property(e => e.ModificadoPor).HasMaxLength(100);
            entity.Property(e => e.Nombre)
                .HasMaxLength(250)
                .HasDefaultValue("");
            entity.Property(e => e.Telefono)
                .HasMaxLength(250)
                .HasDefaultValue("");
            entity.Property(e => e.UsrAct).HasMaxLength(50);
            entity.Property(e => e.UsrReg).HasMaxLength(50);

            entity.HasOne(d => d.IdClienteNavigation).WithMany(p => p.ClienteContactos)
                .HasForeignKey(d => d.IdCliente)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__ClienteCo__IdCli__59063A47");
        });

        modelBuilder.Entity<ClienteRazonSolcial>(entity =>
        {
            entity.HasKey(e => e.IdClienteRazonSolcial).HasName("PK__ClienteR__2A3FED62734EEE20");

            entity.ToTable("ClienteRazonSolcial", "DRO");

            entity.Property(e => e.CreadoPor).HasMaxLength(100);
            entity.Property(e => e.Domicilio)
                .IsUnicode(false)
                .HasDefaultValue("");
            entity.Property(e => e.FchAct).HasColumnType("datetime");
            entity.Property(e => e.FchReg).HasColumnType("datetime");
            entity.Property(e => e.ModificadoPor).HasMaxLength(100);
            entity.Property(e => e.Observaciones)
                .HasMaxLength(600)
                .HasDefaultValue("");
            entity.Property(e => e.RazonSocial)
                .HasMaxLength(250)
                .IsUnicode(false)
                .HasDefaultValue("");
            entity.Property(e => e.Rfc)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasDefaultValue("")
                .HasColumnName("RFC");
            entity.Property(e => e.UsrAct).HasMaxLength(50);
            entity.Property(e => e.UsrReg).HasMaxLength(50);

            entity.HasOne(d => d.IdClienteNavigation).WithMany(p => p.ClienteRazonSolcials)
                .HasForeignKey(d => d.IdCliente)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__ClienteRa__IdCli__5FB337D6");
        });


        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}