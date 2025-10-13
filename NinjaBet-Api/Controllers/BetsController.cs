using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NinjaBet_Application.DTOs;
using NinjaBet_Application.Services;
using NinjaBet_Dmain.Entities;
using NinjaBet_Dmain.Enums;
using System.Security.Claims;
using System.Threading.Tasks;

namespace NinjaBet_Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class BetsController : ControllerBase
    {
        private readonly BetService _betService;
        private readonly UsuarioService _usuarioService;

        public BetsController(BetService betService, UsuarioService usuarioService)
        {
            _betService = betService;
            _usuarioService = usuarioService;
        }

        [HttpPost]
        public async Task<IActionResult> CreateBet([FromBody] BetTicketDto dto)
        {
            var userId = int.Parse(User.FindFirstValue("id")!);

            var apostador = _usuarioService.GetByIdAsync(userId).Result;
            dto.ApostadorId = apostador.Id;
            dto.CambistaId = apostador.CriadorId;
            /*
             ##############
             ## PENDENTE ##
            - colocar regra de negocio para validar se a aposta pode ser criada
            - verificar se o usuario tem saldo suficiente
            - debitar o saldo do usuario
            - verificar se a partida ainda nao comecou
            -verificar se o valor da aposta > zero
            - verificar se a odd informada bate com a odd atual da partida
            - verificar se o tipo de aposta e valido para a partida
             ##############
            */
            var bet = await _betService.CreateBetAsync(dto);
            return Ok(new { success = true, betId = bet.Id });
        }

        [HttpGet("Bilhete/{id}")]
        public async Task<IActionResult> ObterAposta(int id)
        {
            var aposta = await _betService.ObterApostaDetalhadaAsync(id);

            if (aposta == null)
                return NotFound(new { success = false, message = "Aposta não encontrada" });

            return Ok(new { success = true, data = aposta });
        }

        [Authorize(Roles = "Admin,Gerente")]
        [HttpGet("Relatorios")]
        public IActionResult GetRelatorios()
        {
            return Ok("Somente admins e gerentes acessam isso!");
        }


        [Authorize(Roles = "Admin,Cambista")]
        [HttpGet("listarBets")]
        public async Task<IActionResult> listarBets()
        {
            var userId = int.Parse(User.FindFirstValue("id")!);

            var apostas = await _betService.ListarBetsDoCambista(userId);

            return Ok(new { success = true, data = apostas });
        }

        [HttpPost("aprovar/{betId}")]
        [Authorize(Roles = "Cambista")]
        public async Task<IActionResult> AprovarAposta(int betId)
        {
            try
            {
                var usuarioId = int.Parse(User.FindFirst("id")!.Value);
                var perfil = Enum.Parse<PerfilAcessoEnum>(User.FindFirst(ClaimTypes.Role)!.Value);

                if (perfil != PerfilAcessoEnum.Cambista)
                    return Forbid("Apenas Cambistas podem aprovar apostas.");

                var bet = await _betService.AprovarAposta(betId, usuarioId);

                return Ok(new
                {
                    Mensagem = "Aposta aprovada com sucesso!",
                    Aposta = new
                    {
                        bet.Id,
                        bet.Status,
                        bet.DataAprovacao,
                    }
                });

            }
            catch (Exception ex)
            {
                return BadRequest(new { Erro = ex.Message });
            }
        }

        [HttpPost("cancelar/{betId}")]
        [Authorize(Roles = "Cambista")]
        public async Task<IActionResult> CancelarAposta(int betId)
        {
            try
            {
                var userId = int.Parse(User.FindFirstValue("id")!);
                var perfil = Enum.Parse<PerfilAcessoEnum>(User.FindFirst(ClaimTypes.Role)!.Value);

                if (perfil != PerfilAcessoEnum.Cambista)
                    return Forbid("Apenas Cambistas podem aprovar apostas.");

                var aposta = await _betService.CancelarApostaAsync(betId, userId);

                return Ok(new
                {
                    Mensagem = "Aposta cancelada com sucesso!",
                    Aposta = new
                    {
                        aposta.Id,
                        aposta.Status,
                        aposta.DataCancelado
                    }
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { Erro = ex.Message });
            }
        }
    }
}
