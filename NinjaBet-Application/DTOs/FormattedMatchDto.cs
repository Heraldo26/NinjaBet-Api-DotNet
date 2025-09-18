namespace NinjaBet_Application.DTOs
{
    public class FormattedMatchDto
    {
        public int Id { get; set; }
        public string SportType { get; set; } = "Football";
        public string Competition { get; set; } = "";
        public string Team1 { get; set; } = "";
        public string Team2 { get; set; } = "";
        public string? Team1Logo { get; set; }
        public string? Team2Logo { get; set; }
        public string Date { get; set; } = "";
        public string Time { get; set; } = "";
        public string Status { get; set; } = "";
        public int? Elapsed { get; set; }
        public int? PlacarCasa { get; set; }
        public int? PlacarFora { get; set; }
    }
}
