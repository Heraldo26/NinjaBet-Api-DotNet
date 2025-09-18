using NinjaBet_Application.DTOs;

namespace NinjaBet_Application.Interfaces
{
    public interface IJogosService
    {
        Task<FormattedMatchDto?> ObterJogoPorId(int gameId);
    }
}
