using Microsoft.EntityFrameworkCore;
using NinjaBet_Dmain.Entities;
using NinjaBet_Dmain.Entities.Log;
using NinjaBet_Dmain.Enums;

namespace NinjaBet_Infrastructure.Persistence
{
    public class AppDbContext : DbContext
    {
        public DbSet<Bet> Bets { get; set; }
        public DbSet<BetSelecao> BetSelections { get; set; }
        public DbSet<LogErro> Logs { get; set; } = null!;

        public DbSet<Usuario> Usuarios { get; set; }

        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Bet>()
                .HasMany(b => b.Selections)
                .WithOne()
                .OnDelete(DeleteBehavior.Cascade);

            base.OnModelCreating(modelBuilder);

            //Criar usuario padrao admin
            //modelBuilder.Entity<Usuario>().HasData(
            //    new Usuario
            //    {
            //        Id = -1, // valor negativo para seed
            //        Username = "admin",
            //        PasswordHash = BCrypt.Net.BCrypt.HashPassword("admin123"),
            //        Perfil = PerfilAcessoEnum.Admin,
            //        Ativo = true,
            //        DataCriacao = DateTime.UtcNow
            //    }
            //);
        }
    }
}
