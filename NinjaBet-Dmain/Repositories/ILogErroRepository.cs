using NinjaBet_Dmain.Entities.Log;

namespace NinjaBet_Dmain.Repositories
{
    public interface ILogErroRepository
    {
        Task AdicionarAsync(LogErro log);
        Task<List<LogErro>> ListarTodosAsync();
    }
}
