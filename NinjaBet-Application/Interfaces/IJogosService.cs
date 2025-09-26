using NinjaBet_Application.DTOs;

namespace NinjaBet_Application.Interfaces
{
    public interface IJogosService
    {
        Task<List<FormattedMatchDto>> ObterPartidas(string? date = null);
        Task<FormattedMatchDto?> ObterJogoPorId(int gameId);

        //Task<OddsDto> ObterOddsPorJogo(int fixtureId);
        //Task<List<FormattedMatchDto>> ObterPartidasComOdds(string? data = null);
    }
}
