using Microsoft.EntityFrameworkCore;
using NinjaBet_Dmain.Entities.Log;
using NinjaBet_Dmain.Repositories;

namespace NinjaBet_Infrastructure.Persistence.Repositories
{
    public class LogErroRepository : ILogErroRepository
    {
        private readonly AppDbContext _context;

        public LogErroRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task AdicionarAsync(LogErro log)
        {
            await _context.Logs.AddAsync(log);
            await _context.SaveChangesAsync();
        }

        public async Task<List<LogErro>> ListarTodosAsync()
        {
            return await _context.Logs.OrderByDescending(l => l.Data).ToListAsync();
        }
    }
}
