using NinjaBet_Dmain.Entities;

namespace NinjaBet_Dmain.Repositories
{
    public interface IUsuarioRepository
    {
        Task<Usuario?> GetUsuarioAsync(string username, string password);

        Usuario? GetByUsername(string username);
        void Add(Usuario usuario);
    }
}
