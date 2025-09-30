namespace NinjaBet_Api.Models.Jogos
{
    public class FootballMatch
    {
        public Fixture fixture { get; set; }
        public LeagueModel league { get; set; }
        public Teams teams { get; set; }
        public Goals goals { get; set; }
        public Score score { get; set; }
    }
}
