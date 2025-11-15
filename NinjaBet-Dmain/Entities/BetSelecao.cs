using System.ComponentModel.DataAnnotations;

namespace NinjaBet_Dmain.Entities
{
    public class BetSelecao
    {
        [Key]
        public int Id { get; private set; }
        public int IdJogo { get; private set; } //identificador do jogo real que vem da API
        public string Competicao { get; private set; }
        public string TipoEsporte { get; private set; }
        public string Time1 { get; set; }
        public string Time2 { get; set; }
        public string Palpite { get; private set; }
        public decimal OddSelecionado { get; private set; }

        public BetSelecao(int idJogo, string competicao, string tipoEsporte, string time1, string time2, string palpite, decimal oddSelecionado)
        {
            IdJogo = idJogo;
            Competicao = competicao;
            TipoEsporte = tipoEsporte;
            Time1 = time1;
            Time2 = time2;
            Palpite = palpite;
            OddSelecionado = oddSelecionado;
        }
    }
}
