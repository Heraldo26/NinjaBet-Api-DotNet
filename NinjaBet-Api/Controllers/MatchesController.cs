using Microsoft.AspNetCore.Mvc;
using NinjaBet_Api.Services;

namespace NinjaBet_Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class MatchesController : ControllerBase
    {
        private readonly FootballApiService _service;

        public MatchesController(FootballApiService service)
        {
            _service = service;
        }

        [HttpGet]
        public async Task<IActionResult> GetMatches([FromQuery] string? date)
        {
            try
            {
                //var matches = await _service.ObterProximosJogosAsync();
                var matches = await _service.GetPardidas();
                return Ok(new { success = true, data = matches });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "Erro ao buscar partidas", error = ex.Message });
            }
        }
    }
}
