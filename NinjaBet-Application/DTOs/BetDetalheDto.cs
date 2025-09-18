namespace NinjaBet_Application.DTOs
{
    public class BetDetalheDto
    {
        public int Id { get; set; }
        public decimal Valor { get; set; }
        public decimal TotalOdds { get; set; }
        public decimal PossivelRetorno { get; set; }
        public DateTime DataCriada { get; set; }
        public List<BetSelecaoDto> Selecoes { get; set; } = new();
    }
}
