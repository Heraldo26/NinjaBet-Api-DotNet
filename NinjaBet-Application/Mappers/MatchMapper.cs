using NinjaBet_Application.DTOs;
using NinjaBet_Dmain.Entities;

namespace NinjaBet_Application.Mappers
{
    public static class MatchMapper
    {
        public static MatchDto ToDto(Match match)
        {
            return new MatchDto
            {
                Id = match.Id,
                Competition = match.Competition,
                Team1 = match.Team1,
                Team2 = match.Team2,
                Team1Logo = match.Team1Logo,
                Team2Logo = match.Team2Logo,
                Date = match.Date.ToString("yyyy-MM-dd"),
                Time = match.Date.ToString("HH:mm"),
                Status = match.Status,
                Elapsed = match.Elapsed,
                Score = new ScoreResultDto
                {
                    Team1 = match.Score?.Team1,
                    Team2 = match.Score?.Team2
                },
                HalftimeScore = new ScoreResultDto
                {
                    Team1 = match.HalftimeScore?.Team1,
                    Team2 = match.HalftimeScore?.Team2
                },
                Odds = new OddsDto
                {
                    Team1 = match.Odds?.Team1 ?? 0,
                    Draw = match.Odds?.Draw ?? 0,
                    Team2 = match.Odds?.Team2 ?? 0
                }
            };
        }
    }
}
