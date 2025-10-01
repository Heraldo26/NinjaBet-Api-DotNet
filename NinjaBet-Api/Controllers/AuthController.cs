using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using NinjaBet_Api.Models.Auth;
using NinjaBet_Application.Services;
using NinjaBet_Dmain.Entities;
using NinjaBet_Dmain.Enums;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace NinjaBet_Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IConfiguration _config;
        private readonly UsuarioService _usuarioService;

        public AuthController(IConfiguration config, UsuarioService usuarioService)
        {
            _config = config;
            _usuarioService = usuarioService;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginModel request)
        {
            // Aqui você deve validar no banco de dados
            var usuario = await _usuarioService.GetUsuarioAsync(request.Username, request.Password);

            if (usuario is null)
                return Unauthorized("Usuário ou senha inválidos");

            var token = GenerateJwtToken(usuario);

            return Ok(new
            {
                Token = token,
                Perfil = usuario.Perfil.ToString(),
                Ativo = usuario.Ativo,
                CriadoEm = usuario.DataCriacao
            });
        }

        [HttpPost("register")]
        public IActionResult Register([FromBody] RegisterModel request)
        {
            try
            {
                var usuario = _usuarioService.CreateUsuario(request.Username, request.Password, request.Perfil);

                return Ok(new
                {
                    Mensagem = "Usuário registrado com sucesso!",
                    Usuario = new
                    {
                        usuario.Id,
                        usuario.Username,
                        Perfil = usuario.Perfil.ToString(),
                        usuario.Ativo,
                        usuario.DataCriacao
                    }
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { Erro = ex.Message });
            }
        }

        private string GenerateJwtToken(Usuario usuario)
        {
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sub, usuario.Id.ToString()), // ID do usuário
                new Claim(JwtRegisteredClaimNames.UniqueName, usuario.Username), // Nome de usuário
                new Claim(ClaimTypes.Role, usuario.Perfil.ToString()), // PERFIL como Role
                new Claim("ativo", usuario.Ativo.ToString()),   // Status
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()) // ID único do token
            };

            var token = new JwtSecurityToken(
                issuer: _config["Jwt:Issuer"],
                audience: _config["Jwt:Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(int.Parse(_config["Jwt:ExpireMinutes"])),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

    }

}
