using NinjaBet_Application.DTOs;
using NinjaBet_Dmain.Entities;

namespace NinjaBet_Application.Interfaces
{
    public interface IFootballApiService
    {
        //Task<List<MatchDto>> ObterPartidas(string? date = null);
        Task<Match?> ObterJogoPorId(int gameId);

        //teste novo
        Task<List<Match>> GetJogosPorDataAsync(DateTime? data = null);
        Task<List<Match>> GetProximoJogos(int quantidade = 50);
        Task<List<Match>> GetAoVivoAsync();
        Task<List<Match>> GetPrincipaisCampeonatosAsync();
        Task<List<Match>> GetJogosDaLigaAsync(int leagueId);
        List<League> GetMainLeagues();
    }
}
