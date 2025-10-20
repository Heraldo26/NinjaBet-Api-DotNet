using NinjaBet_Dmain.Entities;

namespace NinjaBet_Dmain.Repositories
{
    public interface IUsuarioRepository
    {
        Task<Usuario?> GetUsuarioAsync(string username, string password);

        Task<Usuario?> GetByUsername(string username);
        Task<Usuario?> GetByIdAsync(int id);

        Task<List<Usuario>> GetAllAtivosAsync(int? criadorId);
        Task<List<Usuario>> GetCambistasAtivosAsync(int? vinculoId);
        Task AddAsync(Usuario usuario);

        Task UpdateAsync(Usuario usuario);
    }
}
