using Microsoft.EntityFrameworkCore;
using NinjaBet_Dmain.Entities;
using NinjaBet_Dmain.Enums;
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

        public async Task<Usuario?> GetByIdAsync(int id)
        {
            return await _context.Usuarios.FirstOrDefaultAsync(u => u.Id == id);
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

        public async Task<Usuario?> GetByUsername(string username)
        {
            return await _context.Usuarios.FirstOrDefaultAsync(u => u.Username == username);
        }

        public async Task AddAsync(Usuario usuario)
        {
            _context.Usuarios.Add(usuario);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(Usuario usuario)
        {
            _context.Update(usuario);
            await _context.SaveChangesAsync();
        }

        public async Task<List<Usuario>> GetAllAtivosAsync(int? criadorId)
        {
            return await _context.Usuarios.Where(a => a.Ativo && (criadorId == null || a.CriadorId == criadorId)).ToListAsync();
        }

        public async Task<List<Usuario>> GetCambistasAtivosAsync(int? vinculoId)
        {
            return await _context.Usuarios
                .Where(q => q.Perfil == PerfilAcessoEnum.Cambista && q.Ativo && (vinculoId == null || q.CriadorId == vinculoId))
                .ToListAsync();
        }
    }
}
