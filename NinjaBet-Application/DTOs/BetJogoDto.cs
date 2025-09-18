namespace NinjaBet_Application.DTOs
{
    public class BetJogoDto
    {
        public int GameId { get; set; }
        public string Competicao { get; set; }
        public string TipoEsporte { get; set; }
        public string Palpite { get; set; }
        public decimal OddSelecionado { get; set; }

        // Dados extras da API externa
        public string TimeCasa { get; set; }
        public string TimeFora { get; set; }
        public string? LogoCasa { get; set; }
        public string? LogoFora { get; set; }
        public string Status { get; set; }
        public int? PlacarCasa { get; set; }
        public int? PlacarFora { get; set; }

        // Resultado da seleção
        public bool? Ganhou { get; set; }
    }
}
