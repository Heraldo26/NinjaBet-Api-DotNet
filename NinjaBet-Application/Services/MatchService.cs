using NinjaBet_Application.Interfaces;
using NinjaBet_Dmain.Entities;

namespace NinjaBet_Application.Services
{
    public class MatchService
    {
        private readonly IFootballApiService _footballApiService;

        public MatchService(IFootballApiService footballApiService)
        {
            _footballApiService = footballApiService;
        }

        public async Task<List<Match>> GetJogosPorDataAsync(DateTime? data = null)
        {
            return await _footballApiService.GetJogosPorDataAsync(data);
        }

        public async Task<List<Match>> ObterPartidasPrincipaisAsync()
        {
            return await _footballApiService.GetPrincipaisCampeonatosAsync();
        }

        public async Task<List<Match>> ObterPartidasPorLigaAsync(int leagueId)
        {
            return await _footballApiService.GetJogosDaLigaAsync(leagueId);
        }

        public List<League> ObterLigasPrincipais()
        {
            return _footballApiService.GetMainLeagues();
        }

        public async Task<List<Match>> ObterJogosAoVivoAsync()
        {
            return await _footballApiService.GetAoVivoAsync();
        }

        public async Task<List<Match>> ObterProximosJogosAsync(int quantidade = 50)
        {
            return await _footballApiService.GetProximoJogos(quantidade);
        }

        public async Task<Match?> ObterJogoPorIdAsync(int matchId)
        {
            return await _footballApiService.ObterJogoPorId(matchId);
        }
    }
}
