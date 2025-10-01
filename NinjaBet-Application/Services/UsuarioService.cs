using NinjaBet_Dmain.Entities;
using NinjaBet_Dmain.Enums;
using NinjaBet_Dmain.Repositories;

namespace NinjaBet_Application.Services
{
    public class UsuarioService
    {
        private readonly IUsuarioRepository _usuarioRepository;
        public UsuarioService(IUsuarioRepository usuarioRepository)
        {
            _usuarioRepository = usuarioRepository;
        }

        public async Task<Usuario?> GetUsuarioAsync(string username, string password)
        {
            var usuario = await _usuarioRepository.GetUsuarioAsync(username, password);

            if (usuario == null || !usuario.Ativo)
                return null;

            return usuario;
        }

        public Usuario CreateUsuario(string username, string password, string perfil)
        {
            var existente = _usuarioRepository.GetByUsername(username);
            if (existente != null)
                throw new Exception("Usuário já existe.");

            var usuario = new Usuario(
                                username,
                                BCrypt.Net.BCrypt.HashPassword(password),
                                Enum.Parse<PerfilAcessoEnum>(perfil, true)
                            );

            _usuarioRepository.Add(usuario);

            return usuario;
        }
    }
}
