using NinjaBet_Dmain.Entities;

namespace NinjaBet_Dmain.Extensions
{
    public static class OddsExtensions
    {
        /// <summary>
        /// Retorna a odd correta baseado no tipo de palpite informado.
        /// </summary>
        public static decimal GetOddForBetType(this Odds odds, string betType)
        {
            if (odds == null)
                throw new ArgumentNullException(nameof(odds));

            if (string.IsNullOrWhiteSpace(betType))
                throw new ArgumentException("Tipo de palpite inválido.", nameof(betType));

            betType = betType.ToLowerInvariant();

            return betType switch
            {
                "home" or "casa" or "time1" or "team1" => odds.Team1,
                "away" or "fora" or "time2" or "team2" => odds.Team2,
                "draw" or "empate" or "tie" => odds.Draw,
                _ => throw new ArgumentException($"Palpite inválido: {betType}")
            };
        }
    }
}
