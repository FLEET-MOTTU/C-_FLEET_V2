using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Csharp.Api.Entities;

namespace Csharp.Api.Data
{
    /// <summary>
    /// Contexto EF Core da aplicação.
    /// Define DbSet para entidades do domínio e configurações de mapeamento.
    /// </summary>
    public class AppDbContext : DbContext
    {
        public DbSet<Moto> Motos { get; set; }
        public DbSet<TagBle> TagsBle { get; set; }
        public DbSet<Beacon> Beacons { get; set; }

        // TABELAS SYNC (vindas do Java)
        public DbSet<Funcionario> Funcionarios { get; set; }
        public DbSet<Pateo> Pateos { get; set; }
        public DbSet<Zona> Zonas { get; set; }

        // TABELAS internas do C#
        public DbSet<ZonaRegraStatus> ZonaRegrasStatus { get; set; }
        public DbSet<MotoZonaHistorico> MotoZonasHistorico { get; set; }

        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Beacon
            modelBuilder.Entity<Beacon>()
                .Property(b => b.Ativo)
                .HasConversion(new BoolToZeroOneConverter<short>())
                .HasColumnType("NUMBER(1)");

            modelBuilder.Entity<Beacon>()
                .HasIndex(b => b.BeaconId)
                .IsUnique();

            // relação beacon -> zona (nullable)
            modelBuilder.Entity<Beacon>()
                .HasOne(b => b.Zona)
                .WithMany()
                .HasForeignKey(b => b.ZonaId)
                .IsRequired(false);

            // Moto
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

            modelBuilder.Entity<Moto>()
                .HasIndex(m => m.Placa)
                .IsUnique()
                .HasFilter("\"Placa\" IS NOT NULL");

            // relação moto -> zona (nullable)
            modelBuilder.Entity<Moto>()
                .HasOne(m => m.Zona)
                .WithMany()
                .HasForeignKey(m => m.ZonaId)
                .IsRequired(false);

            // TagBle
            modelBuilder.Entity<TagBle>()
                .HasIndex(tag => tag.CodigoUnicoTag)
                .IsUnique();

            // Funcionario (SYNC)
            modelBuilder.Entity<Funcionario>(entity =>
            {
                entity.ToTable("FUNCIONARIOS_SYNC");
                entity.Property(f => f.Id).ValueGeneratedNever(); // ID vem do Java
                entity.HasIndex(f => f.Email).IsUnique();
                entity.HasIndex(f => f.Telefone).IsUnique();

                entity.HasOne(f => f.Pateo)
                      .WithMany(p => p.Funcionarios)
                      .HasForeignKey(f => f.PateoId);
            });

            // Pateo (SYNC)
            modelBuilder.Entity<Pateo>(entity =>
            {
                entity.ToTable("PATEOS_SYNC");
                entity.Property(p => p.Id).ValueGeneratedNever(); // ID vem do Java
            });

            // Zona (SYNC)
            modelBuilder.Entity<Zona>(entity =>
            {
                entity.ToTable("ZONAS_SYNC");
                entity.Property(z => z.Id).ValueGeneratedNever(); // ID vem do Java

                entity.HasOne(z => z.Pateo)
                      .WithMany(p => p.Zonas)
                      .HasForeignKey(z => z.PateoId);
            });

            // Tabelas internas do C#

            // ZonaRegraStatus
            modelBuilder.Entity<ZonaRegraStatus>(entity =>
            {
                entity.ToTable("ZONA_REGRA_STATUS");
                entity.HasIndex(z => new { z.PateoId, z.StatusMoto, z.Prioridade }).IsUnique();

                entity.HasOne(z => z.Zona)
                      .WithMany()
                      .HasForeignKey(z => z.ZonaId);

                entity.HasOne(z => z.Pateo)
                      .WithMany()
                      .HasForeignKey(z => z.PateoId);
            });

            // MotoZonaHistorico
            modelBuilder.Entity<MotoZonaHistorico>(entity =>
            {
                entity.ToTable("MOTO_ZONA_HIST");
                entity.HasIndex(h => new { h.MotoId, h.EntradaEm });

                entity.HasOne(h => h.Moto)
                      .WithMany()
                      .HasForeignKey(h => h.MotoId);

                entity.HasOne(h => h.Zona)
                      .WithMany()
                      .HasForeignKey(h => h.ZonaId);
            });
        }
    }
}
