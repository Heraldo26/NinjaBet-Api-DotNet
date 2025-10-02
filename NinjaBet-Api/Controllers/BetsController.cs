using Microsoft.AspNetCore.Mvc;
using NinjaBet_Application.DTOs;
using NinjaBet_Application.Services;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using System.Threading.Tasks;

namespace NinjaBet_Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class BetsController : ControllerBase
    {
        private readonly BetService _betService;

        public BetsController(BetService betService)
        {
            _betService = betService;
        }

        [HttpPost]
        public async Task<IActionResult> CreateBet([FromBody] BetTicketDto dto)
        {
            /*
             ##############
             ## PENDENTE ##
            - colocar regra de negocio para validar se a aposta pode ser criada
            - verificar se o usuario tem saldo suficiente
            - debitar o saldo do usuario
            - verificar se a partida ainda nao comecou
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

        [Authorize(Roles = "Admin")]
        [HttpGet("admin-only")]
        public IActionResult SomenteAdmin()
        {
            return Ok("Acesso liberado para ADMIN!");
        }

        [Authorize(Roles = "Cambista")]
        [HttpGet("listarBets")]
        public async Task<IActionResult> listarBets()
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

            var apostas = await _betService.ListarBetsDoCambista(userId);

            return Ok(new { success = true, data = apostas });
        }
    }
}
