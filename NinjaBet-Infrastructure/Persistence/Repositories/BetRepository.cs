using Microsoft.EntityFrameworkCore;
using NinjaBet_Dmain.Entities;
using NinjaBet_Dmain.Repositories;

namespace NinjaBet_Infrastructure.Persistence.Repositories
{
    public class BetRepository : IBetRepository
    {
        private readonly AppDbContext _context;

        public BetRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task AddAsync(Bet bet)
        {
            await _context.Bets.AddAsync(bet);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(Bet bet)
        {
            _context.Update(bet);
            await _context.SaveChangesAsync();
        }

        public async Task<Bet?> ObterPorIdAsync(int id)
        {
            return await _context.Bets
                .Include(b => b.Selections)
                .FirstOrDefaultAsync(b => b.Id == id);
        }

        public async Task<IEnumerable<Bet>> GetBetsByCambistaAsync(int cambistaId)
        {
            return await _context.Bets
                .Include(b => b.Cambista)
                .Where(b => b.CambistaId == cambistaId)
                .ToListAsync();
        }
    }
}
