using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using SEINMX.Context.Database;

namespace SEINMX.Context;

public partial class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Catalogo> Catalogos { get; set; }

    public virtual DbSet<Cliente> Clientes { get; set; }

    public virtual DbSet<ClienteContacto> ClienteContactos { get; set; }

    public virtual DbSet<ClienteRazonSolcial> ClienteRazonSolcials { get; set; }

    public virtual DbSet<Cotizacion> Cotizacions { get; set; }

    public virtual DbSet<CotizacionDetalle> CotizacionDetalles { get; set; }

    public virtual DbSet<Perfil> Perfils { get; set; }

    public virtual DbSet<Producto> Productos { get; set; }

    public virtual DbSet<ProductosImport> ProductosImports { get; set; }

    public virtual DbSet<Proveedor> Proveedors { get; set; }

    public virtual DbSet<Usuario> Usuarios { get; set; }

    public virtual DbSet<VsCliente> VsClientes { get; set; }

    public virtual DbSet<VsCotizacion> VsCotizacions { get; set; }

    public virtual DbSet<VsCotizacionDetalle> VsCotizacionDetalles { get; set; }

    public virtual DbSet<VsProducto> VsProductos { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("VCHAVEZ");

        modelBuilder.Entity<Catalogo>(entity =>
        {
            entity.HasKey(e => e.IdCatalogo).HasName("PK__Catalogo__FD0AC26CB9DD3825");

            entity.ToTable("Catalogo", "CFG");

            entity.Property(e => e.CreadoPor).HasMaxLength(100);
            entity.Property(e => e.Descripcion)
                .HasMaxLength(150)
                .HasDefaultValue("");
            entity.Property(e => e.Description)
                .HasMaxLength(150)
                .HasDefaultValue("");
            entity.Property(e => e.FchAct).HasColumnType("datetime");
            entity.Property(e => e.FchReg).HasColumnType("datetime");
            entity.Property(e => e.ModificadoPor).HasMaxLength(100);
            entity.Property(e => e.Modulo)
                .HasMaxLength(50)
                .HasDefaultValueSql("(NULL)");
            entity.Property(e => e.Nemonico)
                .HasMaxLength(5)
                .HasDefaultValueSql("(NULL)");
            entity.Property(e => e.UsrAct).HasMaxLength(50);
            entity.Property(e => e.UsrReg).HasMaxLength(50);
            entity.Property(e => e.Valor).HasDefaultValue(0);
            entity.Property(e => e.Valor2).HasDefaultValue(0);

            entity.HasOne(d => d.IdCatalogoMtroNavigation).WithMany(p => p.InverseIdCatalogoMtroNavigation)
                .HasForeignKey(d => d.IdCatalogoMtro)
                .HasConstraintName("FK__Catalogo__IdCata__4F7CD00D");
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
            entity.Property(e => e.UsrAct).HasMaxLength(50);
            entity.Property(e => e.UsrReg).HasMaxLength(50);

            entity.HasOne(d => d.IdPerfilNavigation).WithMany(p => p.Clientes)
                .HasForeignKey(d => d.IdPerfil)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Cliente__IdClien__5D95E53A");
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

            entity.Property(e => e.CodigoPostal)
                .HasMaxLength(50)
                .HasDefaultValue("");
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

        modelBuilder.Entity<Cotizacion>(entity =>
        {
            entity.HasKey(e => e.IdCotizacion).HasName("PK__Cotizaci__9A6DA9EF769635C5");

            entity.ToTable("Cotizacion", "INV");

            entity.Property(e => e.CreadoPor).HasMaxLength(100);
            entity.Property(e => e.Descuento).HasColumnType("numeric(18, 4)");
            entity.Property(e => e.FchAct).HasColumnType("datetime");
            entity.Property(e => e.FchReg).HasColumnType("datetime");
            entity.Property(e => e.ModificadoPor).HasMaxLength(100);
            entity.Property(e => e.Observaciones)
                .HasMaxLength(600)
                .HasDefaultValue("");
            entity.Property(e => e.PorcentajeIva)
                .HasColumnType("numeric(18, 4)")
                .HasColumnName("PorcentajeIVA");
            entity.Property(e => e.Tarifa).HasColumnType("numeric(18, 4)");
            entity.Property(e => e.TipoCambio).HasColumnType("numeric(18, 4)");
            entity.Property(e => e.UsrAct).HasMaxLength(50);
            entity.Property(e => e.UsrReg).HasMaxLength(50);
            entity.Property(e => e.UsuarioResponsable).HasMaxLength(25);

            entity.HasOne(d => d.IdClienteNavigation).WithMany(p => p.Cotizacions)
                .HasForeignKey(d => d.IdCliente)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Cotizacio__IdCli__2FCF1A8A");

            entity.HasOne(d => d.IdClienteContactoNavigation).WithMany(p => p.Cotizacions)
                .HasForeignKey(d => d.IdClienteContacto)
                .HasConstraintName("FK__Cotizacio__IdCli__30C33EC3");

            entity.HasOne(d => d.IdClienteRazonSolcialNavigation).WithMany(p => p.Cotizacions)
                .HasForeignKey(d => d.IdClienteRazonSolcial)
                .HasConstraintName("FK__Cotizacio__IdCli__31B762FC");

            entity.HasOne(d => d.IdPerfilNavigation).WithMany(p => p.Cotizacions)
                .HasForeignKey(d => d.IdPerfil)
                .HasConstraintName("FK__Cotizacio__IdPer__607251E5");

            entity.HasOne(d => d.UsuarioResponsableNavigation).WithMany(p => p.Cotizacions)
                .HasPrincipalKey(p => p.Usuario1)
                .HasForeignKey(d => d.UsuarioResponsable)
                .HasConstraintName("FK__Cotizacio__Usuar__2EDAF651");
        });

        modelBuilder.Entity<CotizacionDetalle>(entity =>
        {
            entity.HasKey(e => e.IdCotizacionDetalle).HasName("PK__Cotizaci__6C5616FE3E824FF3");

            entity.ToTable("CotizacionDetalle", "INV");

            entity.Property(e => e.Cantidad).HasColumnType("numeric(18, 4)");
            entity.Property(e => e.CreadoPor).HasMaxLength(100);
            entity.Property(e => e.FchAct).HasColumnType("datetime");
            entity.Property(e => e.FchReg).HasColumnType("datetime");
            entity.Property(e => e.GananciaProveedor).HasColumnType("numeric(18, 4)");
            entity.Property(e => e.ModificadoPor).HasMaxLength(100);
            entity.Property(e => e.Observaciones)
                .HasMaxLength(600)
                .HasDefaultValue("");
            entity.Property(e => e.PorcentajeProveedor).HasColumnType("numeric(18, 4)");
            entity.Property(e => e.PorcentajeProveedorGanancia).HasColumnType("numeric(18, 4)");
            entity.Property(e => e.PrecioCliente).HasColumnType("numeric(18, 4)");
            entity.Property(e => e.PrecioListaMxn)
                .HasColumnType("numeric(18, 4)")
                .HasColumnName("PrecioListaMXN");
            entity.Property(e => e.PrecioProveedor).HasColumnType("numeric(18, 4)");
            entity.Property(e => e.PrecioSein).HasColumnType("numeric(18, 4)");
            entity.Property(e => e.Total).HasColumnType("numeric(18, 4)");
            entity.Property(e => e.UsrAct).HasMaxLength(50);
            entity.Property(e => e.UsrReg).HasMaxLength(50);

            entity.HasOne(d => d.IdCotizacionNavigation).WithMany(p => p.CotizacionDetalles)
                .HasForeignKey(d => d.IdCotizacion)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Cotizacio__IdCot__395884C4");

            entity.HasOne(d => d.IdProductoNavigation).WithMany(p => p.CotizacionDetalles)
                .HasForeignKey(d => d.IdProducto)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Cotizacio__IdPro__3A4CA8FD");
        });

        modelBuilder.Entity<Perfil>(entity =>
        {
            entity.HasKey(e => e.IdPerfil).HasName("PK__ClienteP__E77B83756ED191C2");

            entity.ToTable("Perfil", "DRO");

            entity.Property(e => e.CreadoPor).HasMaxLength(100);
            entity.Property(e => e.FchAct).HasColumnType("datetime");
            entity.Property(e => e.FchReg).HasColumnType("datetime");
            entity.Property(e => e.Identificador).HasMaxLength(50);
            entity.Property(e => e.ModificadoPor).HasMaxLength(100);
            entity.Property(e => e.Perfil1)
                .HasMaxLength(100)
                .HasColumnName("Perfil");
            entity.Property(e => e.Tarifa).HasColumnType("numeric(18, 4)");
            entity.Property(e => e.UsrAct).HasMaxLength(50);
            entity.Property(e => e.UsrReg).HasMaxLength(50);
        });

        modelBuilder.Entity<Producto>(entity =>
        {
            entity.HasKey(e => e.IdProducto).HasName("PK__Producto__098892101B14BE63");

            entity.ToTable("Producto", "INV");

            entity.Property(e => e.ClaveUnidadSat)
                .HasMaxLength(50)
                .HasDefaultValue("")
                .HasColumnName("ClaveUnidadSAT");
            entity.Property(e => e.Codigo).HasMaxLength(50);
            entity.Property(e => e.CreadoPor).HasMaxLength(100);
            entity.Property(e => e.Descripcion).HasMaxLength(500);
            entity.Property(e => e.FchAct).HasColumnType("datetime");
            entity.Property(e => e.FchReg).HasColumnType("datetime");
            entity.Property(e => e.ModificadoPor).HasMaxLength(100);
            entity.Property(e => e.Observaciones)
                .HasMaxLength(600)
                .HasDefaultValue("");
            entity.Property(e => e.PrecioLista).HasColumnType("decimal(18, 4)");
            entity.Property(e => e.UsrAct).HasMaxLength(50);
            entity.Property(e => e.UsrReg).HasMaxLength(50);

            entity.HasOne(d => d.IdProveedorNavigation).WithMany(p => p.Productos)
                .HasForeignKey(d => d.IdProveedor)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("Producto___fkIdProveedor");
        });

        modelBuilder.Entity<ProductosImport>(entity =>
        {
            entity
                .HasNoKey()
                .ToTable("ProductosImport", "INV");

            entity.Property(e => e.Descripción)
                .HasMaxLength(500)
                .HasColumnName("DESCRIPCIÓN");
            entity.Property(e => e.Item)
                .HasMaxLength(200)
                .HasColumnName("ITEM");
            entity.Property(e => e.MonedaRaiz)
                .HasMaxLength(50)
                .HasColumnName("MONEDA RAIZ");
            entity.Property(e => e.PrecioDeLista)
                .HasColumnType("numeric(18, 4)")
                .HasColumnName("PRECIO DE LISTA");
        });

        modelBuilder.Entity<Proveedor>(entity =>
        {
            entity.HasKey(e => e.IdProveedor).HasName("PK__Proveedo__E8B631AF02A17107");

            entity.ToTable("Proveedor", "DRO");

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
            entity.Property(e => e.TarifaGanancia).HasColumnType("numeric(18, 4)");
            entity.Property(e => e.UsrAct).HasMaxLength(50);
            entity.Property(e => e.UsrReg).HasMaxLength(50);
        });

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

        modelBuilder.Entity<VsCliente>(entity =>
        {
            entity
                .HasNoKey()
                .ToView("vsCliente", "DRO");

            entity.Property(e => e.CreadoPor).HasMaxLength(100);
            entity.Property(e => e.Direccion).HasMaxLength(600);
            entity.Property(e => e.FchAct).HasColumnType("datetime");
            entity.Property(e => e.FchReg).HasColumnType("datetime");
            entity.Property(e => e.Identificador).HasMaxLength(50);
            entity.Property(e => e.ModificadoPor).HasMaxLength(100);
            entity.Property(e => e.Nombre).HasMaxLength(250);
            entity.Property(e => e.Observaciones).HasMaxLength(600);
            entity.Property(e => e.Perfil).HasMaxLength(100);
            entity.Property(e => e.Tarifa).HasColumnType("numeric(18, 4)");
            entity.Property(e => e.UsrAct).HasMaxLength(50);
            entity.Property(e => e.UsrReg).HasMaxLength(50);
        });

        modelBuilder.Entity<VsCotizacion>(entity =>
        {
            entity
                .HasNoKey()
                .ToView("vsCotizacion", "INV");

            entity.Property(e => e.Cliente).HasMaxLength(250);
            entity.Property(e => e.Correo).HasMaxLength(250);
            entity.Property(e => e.Cotizacion).HasMaxLength(63);
            entity.Property(e => e.Descuento).HasColumnType("numeric(18, 4)");
            entity.Property(e => e.DescuentoTotal).HasColumnType("numeric(38, 6)");
            entity.Property(e => e.FactorDescuento).HasColumnType("numeric(23, 8)");
            entity.Property(e => e.FactorIva)
                .HasColumnType("numeric(22, 8)")
                .HasColumnName("FactorIVA");
            entity.Property(e => e.Identificador).HasMaxLength(50);
            entity.Property(e => e.Iva)
                .HasColumnType("numeric(38, 6)")
                .HasColumnName("IVA");
            entity.Property(e => e.NombreContacto).HasMaxLength(250);
            entity.Property(e => e.Observaciones).HasMaxLength(600);
            entity.Property(e => e.Perfil).HasMaxLength(100);
            entity.Property(e => e.PorcentajeIva)
                .HasColumnType("numeric(18, 4)")
                .HasColumnName("PorcentajeIVA");
            entity.Property(e => e.RazonSocial)
                .HasMaxLength(250)
                .IsUnicode(false);
            entity.Property(e => e.Responsable).HasMaxLength(50);
            entity.Property(e => e.Rfc)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("RFC");
            entity.Property(e => e.StatusDesc)
                .HasMaxLength(14)
                .IsUnicode(false);
            entity.Property(e => e.SubTotal).HasColumnType("numeric(38, 4)");
            entity.Property(e => e.Tarifa).HasColumnType("numeric(18, 4)");
            entity.Property(e => e.Telefono).HasMaxLength(250);
            entity.Property(e => e.TipoCambio).HasColumnType("numeric(18, 4)");
            entity.Property(e => e.Total).HasColumnType("numeric(38, 6)");
            entity.Property(e => e.UsuarioResponsable).HasMaxLength(25);
        });

        modelBuilder.Entity<VsCotizacionDetalle>(entity =>
        {
            entity
                .HasNoKey()
                .ToView("vsCotizacionDetalle", "INV");

            entity.Property(e => e.Cantidad).HasColumnType("numeric(18, 4)");
            entity.Property(e => e.ClaveUnidadSat)
                .HasMaxLength(50)
                .HasColumnName("ClaveUnidadSAT");
            entity.Property(e => e.Codigo).HasMaxLength(50);
            entity.Property(e => e.Descripcion).HasMaxLength(500);
            entity.Property(e => e.GananciaProveedor).HasColumnType("numeric(18, 4)");
            entity.Property(e => e.Observaciones).HasMaxLength(600);
            entity.Property(e => e.PorcentajeProveedor).HasColumnType("numeric(18, 4)");
            entity.Property(e => e.PorcentajeProveedorGanancia).HasColumnType("numeric(18, 4)");
            entity.Property(e => e.PrecioCliente).HasColumnType("numeric(18, 4)");
            entity.Property(e => e.PrecioLista).HasColumnType("decimal(18, 4)");
            entity.Property(e => e.PrecioListaMxn)
                .HasColumnType("numeric(18, 4)")
                .HasColumnName("PrecioListaMXN");
            entity.Property(e => e.PrecioProveedor).HasColumnType("numeric(18, 4)");
            entity.Property(e => e.PrecioSein).HasColumnType("numeric(18, 4)");
            entity.Property(e => e.Total).HasColumnType("numeric(18, 4)");
        });

        modelBuilder.Entity<VsProducto>(entity =>
        {
            entity
                .HasNoKey()
                .ToView("vsProducto", "INV");

            entity.Property(e => e.ClaveUnidadSat)
                .HasMaxLength(50)
                .HasColumnName("ClaveUnidadSAT");
            entity.Property(e => e.Codigo).HasMaxLength(50);
            entity.Property(e => e.Descripcion).HasMaxLength(500);
            entity.Property(e => e.IdProducto).ValueGeneratedOnAdd();
            entity.Property(e => e.Moneda)
                .HasMaxLength(3)
                .IsUnicode(false);
            entity.Property(e => e.Observaciones).HasMaxLength(600);
            entity.Property(e => e.PrecioLista).HasColumnType("decimal(18, 4)");
            entity.Property(e => e.Proveedor)
                .HasMaxLength(250)
                .IsUnicode(false);
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
