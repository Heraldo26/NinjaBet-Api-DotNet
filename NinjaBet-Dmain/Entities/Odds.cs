namespace NinjaBet_Dmain.Entities
{
    public class Odds
    {
        public decimal Team1 { get; set; }
        public decimal Draw { get; set; }
        public decimal Team2 { get; set; }

        public Odds() { }
        public Odds(decimal team1, decimal draw, decimal team2)
        {
            Team1 = team1;
            Draw = draw;
            Team2 = team2;
        }
    }

}
