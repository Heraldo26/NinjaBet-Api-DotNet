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
            base.OnModelCreating(modelBuilder);

            // Relacionamento Bet -> Apostador
            modelBuilder.Entity<Bet>()
                .HasOne(b => b.Apostador)
                .WithMany() // ou .WithMany(u => u.ApostasFeitas) se quiser a coleção no Usuario
                .HasForeignKey(b => b.ApostadorId)
                .OnDelete(DeleteBehavior.Restrict);

            // Relacionamento Bet -> Cambista
            modelBuilder.Entity<Bet>()
                .HasOne(b => b.Cambista)
                .WithMany() // ou .WithMany(u => u.ApostasGerenciadas)
                .HasForeignKey(b => b.CambistaId)
                .OnDelete(DeleteBehavior.Restrict);


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
