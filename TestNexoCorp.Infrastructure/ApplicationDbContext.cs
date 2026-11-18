using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using TestNexoCorp.Domain;

namespace TestNexoCorp.Infrastructure;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<ExpProductos> ExpProductos { get; set; }
    public DbSet<Categorias> Categorias { get; set; }
    public DbSet<ProductosCategorias> ProductosCategorias { get; set; }
    public DbSet<TiposProductos> TiposProductos { get; set; }
    public DbSet<CodigosBarras> CodigosBarras { get; set; }
    public DbSet<ErpProductos> ErpProductos { get; set; }
    public DbSet<VentasExpress> VentasExpress { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ExpProductos>()
            .HasOne(e => e.TiposProductos)
            .WithOne(t => t.ExpProductos)
            .HasForeignKey<ExpProductos>(e => e.IdTipoProducto)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<ErpProductos>()
            .HasOne(e => e.ExpProductos)
            .WithOne(t => t.ErpProductos)
            .HasForeignKey<ErpProductos>(e => e.IdProducto)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<CodigosBarras>()
            .HasOne(c => c.ExpProductos)
            .WithOne(e => e.CodigosBarras)
            .HasForeignKey<CodigosBarras>(e => e.IdProducto)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<VentasExpress>()
            .HasOne(v => v.ExpProductos)
            .WithOne(e => e.VentasExpress)
            .HasForeignKey<VentasExpress>(e => e.IdProducto)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Categorias>(
            entity =>
            {
                entity.HasKey(c => c.IdCategoria);

                entity.HasOne(c => c.CategoriaPadre)
                    .WithMany()
                    .HasForeignKey(c => c.IdCategoriaPadre)
                    .IsRequired(false)
                    .OnDelete(DeleteBehavior.Restrict);
            });

        modelBuilder.Entity<ProductosCategorias>(
            e =>
            {
                e.HasOne(o => o.ExpProductos)
                    .WithOne(p => p.ProductosCategoria)
                    .HasForeignKey<ProductosCategorias>(o => o.IdProducto)
                    .OnDelete(DeleteBehavior.Cascade);// FK en Order

                e.HasOne(o => o.Categorias)
                    .WithOne(p => p.ProductosCategoria)
                    .HasForeignKey<ProductosCategorias>(o => o.IdCategoria)
                    .OnDelete(DeleteBehavior.Cascade);// FK en Order

                e.HasIndex(x => new { x.IdProducto, x.IdCategoria }).IsUnique();
            });
        // modelBuilder.Entity<ProductosCategorias>()
        //     .HasOne(o => o.ExpProductos)
        //     .WithOne(p => p.ProductosCategoria)
        //     .HasForeignKey<ProductosCategorias>(o => o.IdProducto);// FK en Order
        //
        // modelBuilder.Entity<ProductosCategorias>()
        //     .HasOne(o => o.Categorias)
        //     .WithOne(p => p.ProductosCategoria)
        //     .HasForeignKey<ProductosCategorias>(o => o.IdCategoria);// FK en Order
    }
}