using NinjaBet_Dmain.Enums;
using System.ComponentModel.DataAnnotations;

namespace NinjaBet_Dmain.Entities
{
    public class Bet
    {
        [Key]
        public int Id { get; set; }
        public decimal Valor { get; set; }
        public decimal TotalOdds { get; set; }
        public decimal PossivelRetorno { get; set; }
        public StatusApostaEnum Status { get; set; }
        public DateTime DataCriada { get; set; } = DateTime.UtcNow;
        public DateTime? DataAprovacao { get; set; }
        public DateTime? DataCancelado { get; set; }


        private readonly List<BetSelecao> _selections = new();
        public List<BetSelecao> Selections { get; set; } = new();

        public int? ApostadorId { get; set; } //apostador
        public Usuario Apostador { get; set; } = null!;

        public int? CambistaId { get; set; } //Cambista
        public Usuario Cambista { get; set; } = null!;


        protected Bet() { }

        public Bet(decimal valor, decimal totalOdds, decimal possivelRetorno, int? apostadorId, int? cambistaId)
        {
            Valor = valor;
            TotalOdds = totalOdds;
            PossivelRetorno = possivelRetorno;
            ApostadorId = apostadorId;
            CambistaId = cambistaId;
            Status = StatusApostaEnum.Pendente;
        }

        public void AddSelection(BetSelecao selection)
        {
            Selections.Add(selection);
        }

        #region Metodo
        //public void Aprovar()
        //{
        //    if (Status != StatusApostaEnum.Pendente)
        //        throw new InvalidOperationException("Só é possível aprovar apostas pendentes.");

        //    Status = StatusApostaEnum.Aprovada;
        //    DataAprovacao = DateTime.UtcNow;
        //}

        //public void Cancelar()
        //{
        //    if (Status != StatusApostaEnum.Pendente)
        //        throw new InvalidOperationException("Só é possível cancelar apostas pendentes.");

        //    Status = StatusApostaEnum.Cancelada;
        //    DataCancelado = DateTime.UtcNow;
        //}
        #endregion
    }
}
