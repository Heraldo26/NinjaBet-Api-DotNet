using Microsoft.EntityFrameworkCore;
using NinjaBet_Dmain.Entities;
using NinjaBet_Dmain.Repositories;

namespace NinjaBet_Infrastructure.Persistence.Repositories
{
    public class UsuarioRepository : IUsuarioRepository
    {
        private readonly AppDbContext _context;

        public UsuarioRepository(AppDbContext context)
        {
            _context = context;
        }
        public async Task<Usuario?> GetUsuarioAsync(string username, string password)
        {
            var usuario = await _context.Usuarios
                .FirstOrDefaultAsync(u => u.Username == username);

            if (usuario == null) return null;

            // Verifica hash da senha
            return BCrypt.Net.BCrypt.Verify(password, usuario.PasswordHash)
                ? usuario
                : null;
        }

        public Usuario? GetByUsername(string username)
        {
            return _context.Usuarios.FirstOrDefault(u => u.Username == username);
        }

        public void Add(Usuario usuario)
        {
            _context.Usuarios.Add(usuario);
            _context.SaveChanges();
        }
    }
}
