using Microsoft.Extensions.Logging;
using NinjaBet_Application.DTOs;
using NinjaBet_Dmain.Entities;
using NinjaBet_Dmain.Entities.Log;
using NinjaBet_Dmain.Repositories;

namespace NinjaBet_Application.Services
{
    public class BetService
    {
        private readonly IBetRepository _betRepository;
        private readonly ILogErroRepository _logRepository;
        private readonly ILogger<BetService> _logger;

        public BetService(IBetRepository betRepository, ILogErroRepository logRepository, ILogger<BetService> logger)
        {
            _betRepository = betRepository;
            _logRepository = logRepository;
            _logger = logger;
        }

        public async Task<Bet> CreateBetAsync(BeTicketDto dto)
        {
            try
            {
                var bet = new Bet(dto.ValorAposta, dto.OddTotal, dto.PossivelRetorno);

                foreach (var selectionDto in dto.Selecoes)
                {
                    var selection = new BetSelecao(
                        selectionDto.IdJogo,
                        selectionDto.Competicao,
                        selectionDto.TipoEsporte,
                        selectionDto.Palpite,
                        selectionDto.OddSelecionada
                    );

                    bet.AddSelection(selection);
                }

                await _betRepository.AddAsync(bet);
                return bet;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao criar aposta com stake {IdBilhete}", dto.IdBilhete);

                var log = new LogErro(ex.Message, ex.StackTrace);
                await _logRepository.AdicionarAsync(log);

                throw new ApplicationException("Não foi possível salvar a aposta. Tente novamente.", ex);
            }
        }

        public async Task<Bet?> ObterApostaPorIdAsync(int id)
        {
            try
            {
                return await _betRepository.ObterPorIdAsync(id);
            }
            catch (Exception ex)
            {
                // Opcional: salvar log de erro se falhar
                var log = new LogErro(ex.Message, ex.StackTrace);
                await _logRepository.AdicionarAsync(log);
                throw new ApplicationException("Erro ao buscar a aposta.", ex);
            }
        }
    }
}
