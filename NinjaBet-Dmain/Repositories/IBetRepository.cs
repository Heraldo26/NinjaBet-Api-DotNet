using NinjaBet_Dmain.Entities;

namespace NinjaBet_Dmain.Repositories
{
    public interface IBetRepository
    {
        Task AddAsync(Bet bet);
        Task UpdateAsync(Bet bet);
        Task<Bet?> ObterPorIdAsync(int id);

        Task<IEnumerable<Bet>> GetBetsByCambistaAsync(int cambistaId);

        IQueryable<Bet> GetAll();

        Task<List<int>> GetCambistasByGerenteIdAsync(int gerenteId);
    }
}
