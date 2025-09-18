using Microsoft.Extensions.Logging;
using NinjaBet_Application.DTOs;
using NinjaBet_Application.Interfaces;
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
        private readonly IJogosService _jogosService;

        public BetService(IBetRepository betRepository, ILogErroRepository logRepository, ILogger<BetService> logger, IJogosService jogosService)
        {
            _betRepository = betRepository;
            _logRepository = logRepository;
            _logger = logger;
            _jogosService = jogosService;
        }

        public async Task<Bet> CreateBetAsync(BetTicketDto dto)
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

        public async Task<BetDetalheDto?> ObterApostaDetalhadaAsync(int apostaId)
        {
            var aposta = await _betRepository.ObterPorIdAsync(apostaId);
            if (aposta == null) return null;

            var dto = new BetDetalheDto
            {
                Id = aposta.Id,
                Valor = aposta.Valor,
                TotalOdds = aposta.TotalOdds,
                PossivelRetorno = aposta.PossivelRetorno,
                DataCriada = aposta.DataCriada
            };

            foreach (var sel in aposta.Selections)
            {
                var jogo = await _jogosService.ObterJogoPorId(sel.IdJogo);

                var jogoDto = new BetJogoDto
                {
                    GameId = sel.IdJogo,
                    Competicao = sel.Competicao,
                    TipoEsporte = sel.TipoEsporte,
                    Palpite = sel.Palpite,
                    OddSelecionado = sel.OddSelecionado,
                    TimeCasa = jogo?.Team1 ?? "N/D",
                    TimeFora = jogo?.Team2 ?? "N/D",
                    LogoCasa = jogo?.Team1Logo,
                    LogoFora = jogo?.Team2Logo,
                    Status = jogo?.Status ?? "Desconhecido",
                    PlacarCasa = jogo?.PlacarCasa,
                    PlacarFora = jogo?.PlacarFora,

                    Ganhou = jogo != null && jogo.PlacarCasa.HasValue && jogo.PlacarFora.HasValue
                        ? (sel.Palpite.ToLower() == "team1" && jogo.PlacarCasa > jogo.PlacarFora)
                          || (sel.Palpite.ToLower() == "team2" && jogo.PlacarFora > jogo.PlacarCasa)
                          || (sel.Palpite.ToLower() == "draw" && jogo.PlacarCasa == jogo.PlacarFora)
                        : (bool?)null

                };

                if (jogo != null && jogo.Status == "Match Finished")
                {
                    jogoDto.Ganhou = VerificarResultado(
                        sel.Palpite,
                        jogo.PlacarCasa,
                        jogo.PlacarFora
                    );
                }

                dto.Jogos.Add(jogoDto);
            }

            return dto;
        }

        private bool VerificarResultado(string palpite, int? placarCasa, int? placarFora)
        {
            if (!placarCasa.HasValue || !placarFora.HasValue)
                return false; // ou null se preferir bool?

            return palpite.ToLower() switch
            {
                "team1" => placarCasa > placarFora,
                "team2" => placarFora > placarCasa,
                "draw" => placarCasa == placarFora,
                _ => false
            };
        }
    }
}
