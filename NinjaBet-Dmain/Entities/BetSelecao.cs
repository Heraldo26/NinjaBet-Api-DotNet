namespace NinjaBet_Dmain.Entities
{
    public class BetSelecao
    {
        public Guid Id { get; private set; } = Guid.NewGuid();
        public int IdJogo { get; private set; }
        public string Competicao { get; private set; }
        public string TipoEsporte { get; private set; }
        public string Palpite { get; private set; }
        public decimal OddSelecionado { get; private set; }

        public BetSelecao(int idJogo, string competicao, string tipoEsporte, string palpite, decimal oddSelecionado)
        {
            IdJogo = idJogo;
            Competicao = competicao;
            TipoEsporte = tipoEsporte;
            Palpite = palpite;
            OddSelecionado = oddSelecionado;
        }
    }
}
