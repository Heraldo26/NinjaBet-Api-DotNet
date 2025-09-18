namespace NinjaBet_Dmain.Entities
{
    public class Bet
    {
        public Guid Id { get; private set; } = Guid.NewGuid();
        public decimal Valor { get; private set; }
        public decimal TotalOdds { get; private set; }
        public decimal PossivelRetorno { get; private set; }
        public DateTime DataCriada { get; private set; } = DateTime.UtcNow;

        private readonly List<BetSelecao> _selections = new();
        public List<BetSelecao> Selections { get; private set; } = new();

        public Bet(decimal valor, decimal totalOdds, decimal possivelRetorno)
        {
            Valor = valor;
            TotalOdds = totalOdds;
            PossivelRetorno = possivelRetorno;
        }

        public void AddSelection(BetSelecao selection)
        {
            Selections.Add(selection);
        }
    }
}
