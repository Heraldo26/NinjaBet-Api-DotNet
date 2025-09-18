using Microsoft.EntityFrameworkCore;
using NinjaBet_Dmain.Entities;
using NinjaBet_Dmain.Entities.Log;

namespace NinjaBet_Infrastructure.Persistence
{
    public class AppDbContext : DbContext
    {
        public DbSet<Bet> Bets { get; set; }
        public DbSet<BetSelecao> BetSelections { get; set; }
        public DbSet<LogErro> Logs { get; set; } = null!;

        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Bet>()
                .HasMany(b => b.Selections)
                .WithOne()
                .OnDelete(DeleteBehavior.Cascade);

            base.OnModelCreating(modelBuilder);
        }
    }
}
