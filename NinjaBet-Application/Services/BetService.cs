using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using NinjaBet_Application.DTOs;
using NinjaBet_Application.DTOs.Caixa;
using NinjaBet_Application.Interfaces;
using NinjaBet_Dmain.Entities;
using NinjaBet_Dmain.Entities.Log;
using NinjaBet_Dmain.Enums;
using NinjaBet_Dmain.Extensions;
using NinjaBet_Dmain.Repositories;

namespace NinjaBet_Application.Services
{
    public class BetService
    {
        private readonly IBetRepository _betRepository;
        private readonly ILogErroRepository _logRepository;
        private readonly ILogger<BetService> _logger;
        private readonly IFootballApiService _jogosService;

        public BetService(IBetRepository betRepository, ILogErroRepository logRepository, ILogger<BetService> logger, IFootballApiService jogosService)
        {
            _betRepository = betRepository;
            _logRepository = logRepository;
            _logger = logger;
            _jogosService = jogosService;
        }

        public async Task<IEnumerable<Bet>> ListarBetsDoCambista(int cambistaId)
        {
            return await _betRepository.GetBetsByCambistaAsync(cambistaId);
        }

        public async Task<Bet> CreateBetAsync(BetTicketDto dto)
        {
            try
            {
                var bet = new Bet(dto.ValorAposta, dto.OddTotal, dto.PossivelRetorno, dto.ApostadorId, dto.CambistaId);

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

        public async Task<Bet> AprovarAposta(int betId, int cambistaId)
        {
            var bet = await _betRepository.ObterPorIdAsync(betId);
            if (bet == null)
                throw new ApplicationException("Aposta não encontrada.");

            if (bet.CambistaId != cambistaId)
                throw new Exception("Você não pode aprovar apostas de outro cambista.");

            if (bet.Status != StatusApostaEnum.Pendente)
                throw new ApplicationException("Apenas apostas pendentes podem ser aprovadas.");

            foreach(var selection in bet.Selections)
            {
                var jogo = await _jogosService.ObterJogoPorId(selection.IdJogo);

                if (jogo == null)
                    throw new ApplicationException($"Jogo com ID {selection.IdJogo} não encontrado.");

                if (!string.Equals(jogo.Status, "Not Started", StringComparison.OrdinalIgnoreCase) &&
                    !string.Equals(jogo.Status, "Não Iniciado", StringComparison.OrdinalIgnoreCase))
                    throw new ApplicationException($"Aposta não pode ser aprovada. O jogo {selection.IdJogo} já começou ou terminou.");

                if (jogo.Odds == null)
                    throw new ApplicationException($"Odds não disponíveis para o jogo {selection.IdJogo}.");

                decimal currentOdd;
                try
                {
                    currentOdd = jogo.Odds.GetOddForBetType(selection.Palpite);
                }
                catch (ArgumentException)
                {
                    throw new ApplicationException($"Palpite inválido na seleção {selection.IdJogo}: {selection.Palpite}");
                }

                // Comparação com tolerância: arredonda para 2 casas (odds normalmente tem 2 decimais)
                var oddAposta = decimal.Round(selection.OddSelecionado, 2);
                var oddAtual = decimal.Round(currentOdd, 2);

                if (oddAposta != oddAtual)
                    throw new ApplicationException($"Aposta não pode ser aprovada. A odd para o jogo {selection.IdJogo} mudou. (aposta: {oddAposta}, atual: {oddAtual})");
            }

            bet.Status = StatusApostaEnum.Aprovada;
            bet.DataAprovacao = DateTime.UtcNow;

            await _betRepository.UpdateAsync(bet);

            return bet;
        }

        public async Task<Bet> CancelarApostaAsync(int betId, int cambistaId)
        {
            var bet = await _betRepository.ObterPorIdAsync(betId);
            if (bet == null)
                throw new Exception("Aposta não encontrada.");

            if (bet.CambistaId != cambistaId)
                throw new Exception("Você não pode cancelar apostas de outro cambista.");

            bet.Status = StatusApostaEnum.Cancelada;
            bet.DataCancelado = DateTime.UtcNow;

            await _betRepository.UpdateAsync(bet);

            return bet;
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
                    PlacarCasa = jogo?.Score?.Team1,
                    PlacarFora = jogo?.Score?.Team2,

                    Ganhou = jogo != null && jogo.Score.Team1.HasValue && jogo.Score.Team1.HasValue
                        ? (sel.Palpite.ToLower() == "time1" && jogo.Score.Team1 > jogo.Score.Team2)
                          || (sel.Palpite.ToLower() == "time2" && jogo.Score.Team2 > jogo.Score.Team1)
                          || (sel.Palpite.ToLower() == "empate" && jogo.Score.Team1 == jogo.Score.Team2)
                        : (bool?)null

                };

                if (jogo != null && jogo.Status == "Match Finished")
                {
                    jogoDto.Ganhou = VerificarResultado(
                        sel.Palpite,
                        jogo.Score?.Team1,
                        jogo.Score?.Team2
                    );
                }

                dto.Jogos.Add(jogoDto);
            }

            return dto;
        }

        public async Task<IEnumerable<object>> ConsultarCaixaAsync(int usuarioId, PerfilAcessoEnum perfil, CaixaFiltroDto filtros)
        {
            IQueryable<Bet> query = _betRepository.GetAll();

            if (perfil == PerfilAcessoEnum.Cambista)
            {
                query = query.Where(b => b.CambistaId == usuarioId);
            }
            else if (perfil == PerfilAcessoEnum.Gerente)
            {
                var cambistasIds = await _betRepository.GetCambistasByGerenteIdAsync(usuarioId);
                query = query.Where(b => cambistasIds.Contains(b.CambistaId.Value));
            }

            if(filtros.ClienteId.HasValue)
            {
                query = query.Where(b => b.ApostadorId == filtros.ClienteId.Value);
            }

            if(filtros.DataDe.HasValue)
            {
                query = query.Where(b => b.DataCriada >= filtros.DataDe.Value);
            }

            if(filtros.DataAte.HasValue)
            {
                query = query.Where(b => b.DataCriada <= filtros.DataAte.Value);
            }

            if(filtros.Situacao.HasValue)
            {
                query = query.Where(b => b.Status == filtros.Situacao.Value);
            }

            if(!string.IsNullOrEmpty(filtros.TipoAposta))
            {
                query = filtros.TipoAposta.ToLower() switch
                {
                    "simples" => query.Where(b => b.Selections.Count == 1),
                    "dupla" => query.Where(b => b.Selections.Count == 2),
                    "multipla" => query.Where(b => b.Selections.Count > 2),
                    _ => query
                };
            }

            var lista = await query
                              .Select(b => new
                              {
                                  Numero = b.Id,
                                  Cliente = b.Apostador.Username,
                                  Valor = b.Valor,
                                  Premio = b.PossivelRetorno,
                                  Comissao = Math.Round(b.PossivelRetorno * 0.05m, 2), // Exemplo de comissão de 5%
                                  Situacao = b.Status.ToString(),
                                  Data = b.DataCriada,
                                  Cambista = b.Cambista.Username
                              })
                              .ToListAsync();

            return lista;
        }

        private bool VerificarResultado(string palpite, int? placarCasa, int? placarFora)
        {
            if (!placarCasa.HasValue || !placarFora.HasValue)
                return false;

            return palpite.ToLower() switch
            {
                "time1" => placarCasa > placarFora,
                "time2" => placarFora > placarCasa,
                "empate" => placarCasa == placarFora,
                _ => false
            };
        }
    }
}
