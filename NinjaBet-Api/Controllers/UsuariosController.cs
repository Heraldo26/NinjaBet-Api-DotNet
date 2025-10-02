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

        //[HttpPost("criar-apostador")]
        //[Authorize(Roles = "Cambista")]
        //public async Task<IActionResult> CriarApostador([FromBody] LoginModel request)
        //{
        //    var cambistaId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

        //    var usuario = await _usuarioService.CriarApostadorAsync(cambistaId, request.Username, request.Password);

        //    return Ok(new
        //    {
        //        usuario.Id,
        //        usuario.Username,
        //        usuario.Perfil,
        //        usuario.Ativo,
        //        usuario.DataCriacao,
        //        VinculadoAoCambista = cambistaId
        //    });
        //}

        [HttpPost("criarUsuario")]
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

        [HttpGet("usuarios")]
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
    }
}
