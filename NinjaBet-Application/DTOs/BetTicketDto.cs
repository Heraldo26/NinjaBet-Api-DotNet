namespace NinjaBet_Application.DTOs
{
    public class BetTicketDto
    {
        public decimal IdBilhete { get; set; }
        public decimal OddTotal { get; set; }
        public decimal ValorAposta { get; set; }
        public decimal PossivelRetorno { get; set; }
        public List<BetSelecaoDto> Selecoes { get; set; } = new();
    }
}
