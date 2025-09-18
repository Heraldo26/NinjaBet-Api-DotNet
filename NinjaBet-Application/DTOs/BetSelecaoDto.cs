namespace NinjaBet_Application.DTOs
{
    public class BetSelecaoDto
    {
        public int IdJogo { get; set; }
        public string Competicao { get; set; } = string.Empty;
        public string TipoEsporte { get; set; } = string.Empty;
        public string Time1 { get; set; } = string.Empty;
        public string Time2 { get; set; } = string.Empty;
        public string Palpite { get; set; } = string.Empty;
        public decimal OddSelecionada { get; set; }
    }
}
