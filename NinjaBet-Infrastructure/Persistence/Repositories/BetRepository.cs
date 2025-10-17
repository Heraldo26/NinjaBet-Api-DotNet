using Microsoft.EntityFrameworkCore;
using NinjaBet_Dmain.Entities;
using NinjaBet_Dmain.Enums;
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
            var teste = _context.Bets.ToList();

            return await _context.Bets
                .Include(b => b.Apostador)
                .Include(b => b.Cambista)
                .Include(b => b.Selections)
                .Where(b => b.CambistaId == cambistaId)
                .ToListAsync();
        }

        public IQueryable<Bet> GetAll()
        {
            return _context.Bets
                .Include(b => b.Apostador)
                .Include(b => b.Cambista)
                .Include(b => b.Selections)
                .AsQueryable();
        }

        public async Task<List<int>> GetCambistasByGerenteIdAsync(int gerenteId)
        {
            return await _context.Usuarios
                .Where(u => u.CriadorId == gerenteId && u.Perfil == PerfilAcessoEnum.Cambista)
                .Select(u => u.Id)
                .ToListAsync();
        }
    }
}
