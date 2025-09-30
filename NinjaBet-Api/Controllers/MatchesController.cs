using Microsoft.AspNetCore.Mvc;
using NinjaBet_Api.Services;
using NinjaBet_Application.Interfaces;
using NinjaBet_Application.Services;
using NinjaBet_Dmain.Entities;

namespace NinjaBet_Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class MatchesController : ControllerBase
    {
        private readonly MatchService _matchService;

        public MatchesController(MatchService matchService)
        {
            _matchService = matchService;
        }

        /// <summary>
        /// Retorna os jogos por data
        /// </summary>
        [HttpGet("JogosPorData")]
        public async Task<IActionResult> GetJogosPorData([FromQuery] DateTime? data = null)
        {
            var jogos = await _matchService.GetJogosPorDataAsync(data);
            return Ok(new { data = jogos });
        }

        /// <summary>
        /// Retorna os jogos das ligas principais (home)
        /// </summary>
        [HttpGet("principais")]
        public async Task<ActionResult<List<Match>>> GetPrincipais()
        {
            var partidas = await _matchService.ObterPartidasPrincipaisAsync();
            return Ok(partidas);
        }

        /// <summary>
        /// Retorna todas as ligas principais
        /// </summary>
        [HttpGet("ligas-principais")]
        public ActionResult<List<League>> GetLigasPrincipais()
        {
            var ligas = _matchService.ObterLigasPrincipais();
            return Ok(ligas);
        }

        /// <summary>
        /// Retorna os próximos jogos de uma liga específica
        /// </summary>
        [HttpGet("liga/{leagueId}")]
        public async Task<ActionResult<List<Match>>> GetPorLiga(int leagueId)
        {
            var partidas = await _matchService.ObterPartidasPorLigaAsync(leagueId);
            return Ok(partidas);
        }

        /// <summary>
        /// Retorna os jogos ao vivo
        /// </summary>
        [HttpGet("ao-vivo")]
        public async Task<ActionResult<List<Match>>> GetAoVivo()
        {
            var partidas = await _matchService.ObterJogosAoVivoAsync();
            return Ok(partidas);
        }

        /// <summary>
        /// Retorna as próximas partidas de todas as ligas principais
        /// </summary>
        [HttpGet("proximos")]
        public async Task<ActionResult<List<Match>>> GetProximosJogos()
        {
            try
            {
                var partidas = await _matchService.ObterProximosJogosAsync();
                return Ok(partidas);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "Erro ao buscar próximos jogos", error = ex.Message });
            }
        }

        /// <summary>
        /// Retorna detalhes de um jogo específico
        /// </summary>
        [HttpGet("{matchId}")]
        public async Task<ActionResult<Match>> GetPorId(int matchId)
        {
            var partida = await _matchService.ObterJogoPorIdAsync(matchId);
            if (partida == null) return NotFound();
            return Ok(partida);
        }

    }
}
