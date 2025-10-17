using NinjaBet_Dmain.Enums;

namespace NinjaBet_Api.Models.Caixa
{
    public class CaixaFiltroModel
    {
        public int? ClienteId { get; set; } // ID do apostador
        public int? CambistaId { get; set; } // ID do Cambista
        public DateTime? DataDe { get; set; }
        public DateTime? DataAte { get; set; }
        public StatusApostaEnum? Situacao { get; set; }
        public string? TipoAposta { get; set; } // "Simples", "Dupla", "Multipla"
    }
}
