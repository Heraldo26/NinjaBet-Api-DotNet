using Microsoft.AspNetCore.Mvc;
using NinjaBet_Application.DTOs;
using NinjaBet_Application.Services;
using Microsoft.AspNetCore.Authorization;

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

        [Authorize]
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
    }
}
