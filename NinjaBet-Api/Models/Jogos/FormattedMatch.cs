using Microsoft.AspNetCore.Cors.Infrastructure;

namespace NinjaBet_Api.Models.Jogos
{
    public class FormattedMatch
    {
        public int id { get; set; }
        public string sportType { get; set; } = "Football";
        public string competition { get; set; }
        public string team1 { get; set; }
        public string team2 { get; set; }
        public string? team1Logo { get; set; }
        public string? team2Logo { get; set; }
        public string date { get; set; }
        public string time { get; set; }
        public string status { get; set; }
        public int? elapsed { get; set; }
        public ScoreResult score { get; set; }
        public ScoreResult halftimeScore { get; set; }
        public Odds odds { get; set; }
    }
}
