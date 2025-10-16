using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NinjaBet_Api.Models.Auth;
using NinjaBet_Application.Services;
using NinjaBet_Dmain.Enums;
using System.Security.Claims;

namespace NinjaBet_Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UsuariosController : ControllerBase
    {
        private readonly UsuarioService _usuarioService;

        public UsuariosController(UsuarioService usuarioService)
        {
            _usuarioService = usuarioService;
        }


        [HttpPost("CriarUsuario")]
        [Authorize]
        public async Task<IActionResult> CriarUsuario([FromBody] CreateUsuarioModel request)
        {
            var usuarioCriadorId = int.Parse(User.FindFirstValue("id")!);
            var perfilCriador = Enum.Parse<PerfilAcessoEnum>(User.FindFirstValue("perfil")!);

            // Checa se o criador pode criar o perfil desejado
            if (!UsuarioService.PodeCriarPerfil(perfilCriador, Enum.Parse<PerfilAcessoEnum>(request.Perfil)))
            {
                return Forbid("Você não tem permissão para criar um usuário com esse perfil.");
            }

            var usuario = await _usuarioService.CreateUsuario(request.Username, request.Password, request.Perfil, usuarioCriadorId);

            return Ok(new
            {
                Mensagem = "Usuário criado com sucesso!",
                Usuario = new { usuario.Id, usuario.Username, usuario.Perfil }
            });
        }

        [HttpGet("ListarUsuarios")]
        [Authorize]
        public async Task<IActionResult> ListarUsuarios()
        {
            var usuarioId = int.Parse(User.FindFirstValue("id")!);
            var perfil = Enum.Parse<PerfilAcessoEnum>(User.FindFirstValue("perfil")!);

            // Pega o usuário logado
            var usuarioLogado = await _usuarioService.GetByIdAsync(usuarioId);
            if (usuarioLogado == null || !usuarioLogado.Ativo)
                return Unauthorized();

            var usuarios = await _usuarioService.GetUsuariosVinculadosAsync(usuarioLogado);

            var resultado = usuarios.Select(u => new
            {
                u.Id,
                u.Username,
                Perfil = u.Perfil.ToString(),
                u.Ativo,
                u.DataCriacao,
                CriadorId = u.CriadorId
            });

            return Ok(resultado);
        }

        [HttpPost("EditarUsuario")]
        [Authorize]
        public async Task<IActionResult> EditarUsuario([FromBody] EditUsuarioModel request)
        {
            var idLogado = int.Parse(User.FindFirstValue("id")!);
            var usuarioLogado = await _usuarioService.GetByIdAsync(idLogado);
            if (usuarioLogado == null || !usuarioLogado.Ativo)
                return Unauthorized();

            var userEditado = await _usuarioService.EditarUsuarioVinculadoAsync(idLogado, request.Id, request.Username, request.Password, request.Perfil);

            return Ok(userEditado);
        }

        [HttpGet("Perfis")]
        [Authorize]
        public async Task<IActionResult> ListarPerfis()
        {
            var perfilUsuarioLogado = Enum.Parse<PerfilAcessoEnum>(User.FindFirstValue(ClaimTypes.Role)!);

            IEnumerable<PerfilAcessoEnum> perfisDisponiveis = perfilUsuarioLogado switch
            {
                PerfilAcessoEnum.Admin => Enum.GetValues<PerfilAcessoEnum>(),
                PerfilAcessoEnum.Gerente => new[] { PerfilAcessoEnum.Cambista, PerfilAcessoEnum.Apostador },
                PerfilAcessoEnum.Cambista => new[] { PerfilAcessoEnum.Apostador },
                _ => Array.Empty<PerfilAcessoEnum>()
            };

            var result = perfisDisponiveis.Select(p => new
            {
                Id = (int)p,
                Nome = p.ToString()
            });

            return Ok(result);
        }

        [HttpDelete("ExcluirUsuario/{usuarioId}")]
        [Authorize]
        public async Task<IActionResult> ExcluirUsuario(int usuarioId)
        {
            var usuarioLogadoId = int.Parse(User.FindFirstValue("id")!);
            var perfilLogado = Enum.Parse<PerfilAcessoEnum>(User.FindFirstValue("perfil")!);

            if (usuarioId == usuarioLogadoId)
                return BadRequest("Você não pode excluir a si mesmo.");

            var usuarioParaExcluir = await _usuarioService.GetByIdAsync(usuarioId);
            if (usuarioParaExcluir == null)
                return NotFound("Usuário não encontrado.");

            if (!UsuarioService.PodeCriarPerfil(perfilLogado, usuarioParaExcluir.Perfil))            
                return Forbid("Você não tem permissão para excluir este usuário.");

            var result = await _usuarioService.ExcluirUsuarioAsync(usuarioId);

            return Ok(new { Mensagem = "Usuário excluído com sucesso.", result });

        }
    }
}
