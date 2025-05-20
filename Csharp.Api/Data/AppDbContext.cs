using Microsoft.EntityFrameworkCore;
using Csharp.Api.Entities;
using Csharp.Api.Entities.Enums;

namespace Csharp.Api.Data
{
    public class AppDbContext : DbContext
    {
        public DbSet<Moto> Motos { get; set; }
        public DbSet<TagBle> TagsBle { get; set; }

        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

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
        }
    }
}
