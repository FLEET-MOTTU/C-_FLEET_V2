using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Csharp.Api.Entities;

namespace Csharp.Api.Data
{
    public class AppDbContext : DbContext
    {
        public DbSet<Moto> Motos { get; set; }
        public DbSet<TagBle> TagsBle { get; set; }
        public DbSet<Beacon> Beacons { get; set; }


        // TABELAS SYNC
        public DbSet<Funcionario> Funcionarios { get; set; }
        public DbSet<Pateo> Pateos { get; set; }
        public DbSet<Zona> Zonas { get; set; }

        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Beacon>()
                .Property(b => b.Ativo)
                .HasConversion(new BoolToZeroOneConverter<short>())
                .HasColumnType("NUMBER(1)");

            modelBuilder.Entity<Moto>()
                .Property(m => m.StatusMoto)
                .HasConversion<string>()
                .HasMaxLength(50);

            modelBuilder.Entity<Moto>()
                .Property(m => m.Modelo)
                .HasConversion<string>()
                .HasMaxLength(50);

            modelBuilder.Entity<Moto>()
                .HasOne(moto => moto.Tag)
                .WithOne(tag => tag.Moto)
                .HasForeignKey<Moto>(moto => moto.TagBleId);

            modelBuilder.Entity<TagBle>()
                .HasIndex(tag => tag.CodigoUnicoTag)
                .IsUnique();

            modelBuilder.Entity<Beacon>()
                .HasIndex(b => b.BeaconId)
                .IsUnique();

            modelBuilder.Entity<Funcionario>(entity =>
            {
                entity.ToTable("FUNCIONARIOS_SYNC");
                entity.Property(f => f.Id).ValueGeneratedNever(); // ID vem do Java
                entity.HasIndex(f => f.Email).IsUnique();
                entity.HasIndex(f => f.Telefone).IsUnique();
                // Relacionamento: Um Pateo tem Muitos Funcionarios
                entity.HasOne(f => f.Pateo)
                      .WithMany(p => p.Funcionarios)
                      .HasForeignKey(f => f.PateoId);
            });

            modelBuilder.Entity<Pateo>(entity =>
            {
                entity.ToTable("PATEOS_SYNC");
                entity.Property(p => p.Id).ValueGeneratedNever(); // ID vem do Java
            });

            modelBuilder.Entity<Zona>(entity =>
            {
                entity.ToTable("ZONAS_SYNC");
                entity.Property(z => z.Id).ValueGeneratedNever(); // ID vem do Java
                // Relacionamento: Um Pateo tem Muitas Zonas
                entity.HasOne(z => z.Pateo)
                      .WithMany(p => p.Zonas)
                      .HasForeignKey(z => z.PateoId);
            });
                
        }
    }
}