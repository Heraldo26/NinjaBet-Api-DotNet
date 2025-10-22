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

        public async Task<Usuario?> GetByIdAsync(int id)
        {
            var usuario = await _usuarioRepository.GetByIdAsync(id);
            if (usuario == null)
                throw new Exception("Usuário não encontrado.");

            return usuario;
        }

        public async Task<Usuario?> GetByUsernameAsync(string username)
        {
            return await _usuarioRepository.GetByUsername(username);
        }

        public async Task<Usuario?> GetUsuarioAsync(string username, string password)
        {
            var usuario = await _usuarioRepository.GetUsuarioAsync(username, password);

            if (usuario == null || !usuario.Ativo)
                return null;

            return usuario;
        }

        public async Task<Usuario> CreateUsuario(string username, string password, string perfilDesejado, decimal? saldo, int criadorId)
        {
            Usuario existente = await _usuarioRepository.GetByUsername(username);
            if (existente != null)
                throw new Exception("Usuário já existe.");

            // Converte perfil
            if (!Enum.TryParse<PerfilAcessoEnum>(perfilDesejado, true, out var perfil))
                throw new Exception("Perfil inválido.");


            var usuario = new Usuario
            {
                Username = username,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(password),
                Perfil = perfil,
                Ativo = true,
                DataCriacao = DateTime.UtcNow,
                Saldo = saldo,
                CriadorId = criadorId // vínculo com quem criou
            };

            await _usuarioRepository.AddAsync(usuario);

            return usuario;
        }

        public async Task<Usuario> EditarPerfilAsync(int idUsuario, string username, string password)
        {
            var usuario = await _usuarioRepository.GetByIdAsync(idUsuario);
            if (usuario is null)
                throw new Exception("Usuário não encontrado");

            usuario.Username = username;

            if (!String.IsNullOrEmpty(password))
                usuario.PasswordHash = BCrypt.Net.BCrypt.HashPassword(password);

            await _usuarioRepository.UpdateAsync(usuario);

            return usuario;
        }

        public static bool PodeCriarPerfil(PerfilAcessoEnum criador, PerfilAcessoEnum desejado)
        {
            return criador switch
            {
                PerfilAcessoEnum.Admin => true, // Admin pode criar qualquer perfil
                PerfilAcessoEnum.Gerente => desejado == PerfilAcessoEnum.Cambista || desejado == PerfilAcessoEnum.Apostador,
                PerfilAcessoEnum.Cambista => desejado == PerfilAcessoEnum.Apostador,
                _ => false // Apostador não pode criar usuários
            };
        }

        public async Task<List<Usuario>> GetUsuariosVinculadosAsync(Usuario solicitante)
        {
            return solicitante.Perfil == PerfilAcessoEnum.Admin
                ? await _usuarioRepository.GetAllAtivosAsync(null)
                : await _usuarioRepository.GetAllAtivosAsync(solicitante.Id);
        }

        public async Task<List<Usuario>> GetCambistasVinculadosAsync(Usuario solicitante)
        {
            return solicitante.Perfil == PerfilAcessoEnum.Admin
                 ? await _usuarioRepository.GetCambistasAtivosAsync(null)
                 : await _usuarioRepository.GetCambistasAtivosAsync(solicitante.Id);
        }

        public async Task<Usuario> EditarUsuarioVinculadoAsync(int idLogado, int idUsuario, string username, decimal? saldo, string password, string perfilDesejado)
        {
            var usuario = await _usuarioRepository.GetByIdAsync(idUsuario);
            if (usuario is null)
                throw new Exception("Usuário não encontrado");

            if (usuario.CriadorId != idLogado)
                throw new Exception("Usuário não está na sua lista de Usuários");

            if (!Enum.TryParse<PerfilAcessoEnum>(perfilDesejado, true, out var perfil))
                throw new Exception("Perfil inválido.");

            usuario.Username = username;
            usuario.Perfil = perfil;
            usuario.Saldo = saldo;
            if (!String.IsNullOrEmpty(password))
                usuario.PasswordHash = BCrypt.Net.BCrypt.HashPassword(password);

            await _usuarioRepository.UpdateAsync(usuario);

            return usuario;
        }

        public async Task<Usuario> ExcluirUsuarioAsync(int usuarioId)
        {
            var usuario = await GetByIdAsync(usuarioId);

            usuario.Ativo = false;

            await _usuarioRepository.UpdateAsync(usuario);

            return usuario;
        }
    }
}
