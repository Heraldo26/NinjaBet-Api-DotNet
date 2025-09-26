using NinjaBet_Api.Models.Jogos;
using NinjaBet_Application.DTOs;
using NinjaBet_Application.Interfaces;
using System.Text.Json;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace NinjaBet_Api.Services
{
    public class FootballApiService : IJogosService
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _config;

        public FootballApiService(HttpClient httpClient, IConfiguration config)
        {
            _httpClient = httpClient;
            _config = config;

            _httpClient.DefaultRequestHeaders.Add("X-RapidAPI-Key", _config["Football:RAPIDAPI_KEY"]);
            _httpClient.DefaultRequestHeaders.Add("X-RapidAPI-Host", _config["Football:RAPIDAPI_HOST"]);
        }

        public async Task<List<FormattedMatchDto>> GetPardidas(int quantidade = 50)
        {
            List<FormattedMatchDto> partidasFutebol = new List<FormattedMatchDto>();

            partidasFutebol = await ObterProximosJogosAsync(quantidade);
            partidasFutebol.AddRange( await ObterJogosAoVivoAsync());

            return partidasFutebol;
        }

        public async Task<List<FormattedMatchDto>> ObterProximosJogosAsync(int quantidade = 50)
        {
            var baseUrl = _config["Football:RAPIDAPI_FOOTBALL_URL"];
            var url = $"{baseUrl}/fixtures?next={quantidade}";
            return await ProcessarPartidasAsync(url);
        }

        public async Task<List<FormattedMatchDto>> ObterJogosAoVivoAsync()
        {
            var baseUrl = _config["Football:RAPIDAPI_FOOTBALL_URL"];
            var url = $"{baseUrl}/fixtures?live=all";
            return await ProcessarPartidasAsync(url);
        }

        /// <summary>
        /// Método interno que processa as partidas e chama odds
        /// </summary>
        private async Task<List<FormattedMatchDto>> ProcessarPartidasAsync(string url)
        {
            try
            {
                var response = await _httpClient.GetAsync(url);
                response.EnsureSuccessStatusCode();

                var json = await response.Content.ReadAsStringAsync();
                using var doc = JsonDocument.Parse(json);

                var partidas = new List<FormattedMatchDto>();

                foreach (var item in doc.RootElement.GetProperty("response").EnumerateArray())
                {

                    var fixture = item.GetProperty("fixture");
                    var leagueObj = item.GetProperty("league");
                    var teams = item.GetProperty("teams");
                    var goals = item.GetProperty("goals");

                    var matchDate = DateTime.Parse(fixture.GetProperty("date").GetString() ?? DateTime.Now.ToString());
                    var fixtureId = fixture.GetProperty("id").GetInt32();

                    var odds = await ObterOddsDaPartidaAsync(fixtureId);

                    // Verifica se elapsed existe e não é null
                    int? elapsed = null;
                    if (fixture.GetProperty("status").TryGetProperty("elapsed", out var elapsedProp) &&
                        elapsedProp.ValueKind == JsonValueKind.Number)
                    {
                        elapsed = elapsedProp.GetInt32();
                    }

                    // Verifica gols antes de acessar
                    int? homeGoals = null;
                    int? awayGoals = null;

                    if (goals.TryGetProperty("home", out var homeProp) && homeProp.ValueKind == JsonValueKind.Number)
                        homeGoals = homeProp.GetInt32();

                    if (goals.TryGetProperty("away", out var awayProp) && awayProp.ValueKind == JsonValueKind.Number)
                        awayGoals = awayProp.GetInt32();

                    partidas.Add(new FormattedMatchDto
                    {
                        Id = fixtureId,
                        Competition = leagueObj.GetProperty("name").GetString() ?? "",
                        Team1 = teams.GetProperty("home").GetProperty("name").GetString() ?? "",
                        Team2 = teams.GetProperty("away").GetProperty("name").GetString() ?? "",
                        Team1Logo = teams.GetProperty("home").GetProperty("logo").GetString(),
                        Team2Logo = teams.GetProperty("away").GetProperty("logo").GetString(),
                        Date = matchDate.ToString("yyyy-MM-dd"),
                        Time = matchDate.ToString("HH:mm"),
                        Status = fixture.GetProperty("status").GetProperty("long").GetString() ?? "Desconhecido",
                        Elapsed = elapsed,
                        Score = new ScoreResultDto
                        {
                            Team1 = homeGoals,
                            Team2 = awayGoals
                        },
                        HalftimeScore = new ScoreResultDto
                        {
                            Team1 = 0,
                            Team2 = 0
                        },
                        Odds = odds
                    });
                }

                return partidas;

            }
            catch (Exception ex)
            {
                throw new Exception($"Erro ao obter partida : {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Busca as odds de uma partida específica usando o endpoint de odds
        /// </summary>
        private async Task<OddsDto> ObterOddsDaPartidaAsync(int fixtureId)
        {
            try
            {
                var baseUrl = _config["Football:RAPIDAPI_FOOTBALL_URL"];
                var url = $"{baseUrl}/odds?fixture={fixtureId}";

                var response = await _httpClient.GetAsync(url);
                if (!response.IsSuccessStatusCode)
                    return new OddsDto { Team1 = 0, Draw = 0, Team2 = 0 };

                var json = await response.Content.ReadAsStringAsync();
                using var doc = JsonDocument.Parse(json);

                var responseArray = doc.RootElement.GetProperty("response");
                if (responseArray.GetArrayLength() == 0)
                    return new OddsDto { Team1 = 0, Draw = 0, Team2 = 0 };

                // Pega o primeiro bookmaker disponível
                var bookmaker = responseArray[0].GetProperty("bookmakers")[0];
                var bets = bookmaker.GetProperty("bets");

                // Busca a aposta de vencedor da partida (1x2)
                var h2hBet = bets.EnumerateArray()
                    .FirstOrDefault(b => b.GetProperty("name").GetString() == "Match Winner");

                if (h2hBet.ValueKind == JsonValueKind.Undefined)
                    return new OddsDto { Team1 = 0, Draw = 0, Team2 = 0 };

                var values = h2hBet.GetProperty("values").EnumerateArray();
                decimal team1 = 0, draw = 0, team2 = 0;

                foreach (var v in values)
                {
                    var type = v.GetProperty("value").GetString();
                    var oddString = v.GetProperty("odd").GetString();
                    decimal.TryParse(oddString, out var oddDecimal);

                    switch (type)
                    {
                        case "Home":
                            team1 = oddDecimal;
                            break;
                        case "Draw":
                            draw = oddDecimal;
                            break;
                        case "Away":
                            team2 = oddDecimal;
                            break;
                    }
                }

                return new OddsDto
                {
                    Team1 = team1,
                    Draw = draw,
                    Team2 = team2
                };
            }
            catch (Exception ex)
            {
                throw new Exception($"Erro ao obter odds do jogo {fixtureId}: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Retorna todas as partidas da liga/temporada (opcional por data)
        /// </summary>
        public async Task<List<FormattedMatchDto>> ObterPartidas(string? date = null)
        {
            var baseUrl = _config["Football:RAPIDAPI_FOOTBALL_URL"];
            var league = _config["Football:RAPIDAPI_FOOTBALL_LEAGUE"];
            var season = _config["Football:RAPIDAPI_FOOTBALL_SEASON"];

            string url = $"{baseUrl}/fixtures?league={league}&season={season}";
            if (!string.IsNullOrEmpty(date))
                url += $"&date={date}";

            var response = await _httpClient.GetAsync(url);
            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(json);

            var partidas = new List<FormattedMatchDto>();
            foreach (var item in doc.RootElement.GetProperty("response").EnumerateArray())
            {
                var fixture = item.GetProperty("fixture");
                var leagueObj = item.GetProperty("league");
                var teams = item.GetProperty("teams");
                var goals = item.GetProperty("goals");
                var score = item.GetProperty("score");

                var matchDate = DateTime.Parse(fixture.GetProperty("date").GetString() ?? DateTime.Now.ToString());

                var partida = new FormattedMatchDto
                {
                    Id = fixture.GetProperty("id").GetInt32(),
                    Competition = leagueObj.GetProperty("name").GetString() ?? "",
                    Team1 = teams.GetProperty("home").GetProperty("name").GetString() ?? "",
                    Team2 = teams.GetProperty("away").GetProperty("name").GetString() ?? "",
                    Team1Logo = teams.GetProperty("home").GetProperty("logo").GetString(),
                    Team2Logo = teams.GetProperty("away").GetProperty("logo").GetString(),
                    Date = matchDate.ToString("yyyy-MM-dd"),
                    Time = matchDate.ToString("HH:mm"),
                    Status = fixture.GetProperty("status").GetProperty("long").GetString() ?? "Desconhecido",
                    Elapsed = fixture.GetProperty("status").TryGetProperty("elapsed", out var elapsed) ? elapsed.GetInt32() : null,
                    Score = new ScoreResultDto
                    {
                        Team1 = goals.GetProperty("home").ValueKind == JsonValueKind.Null ? null : goals.GetProperty("home").GetInt32(),
                        Team2 = goals.GetProperty("away").ValueKind == JsonValueKind.Null ? null : goals.GetProperty("away").GetInt32()
                    },
                    HalftimeScore = new ScoreResultDto
                    {
                        Team1 = score.GetProperty("halftime").GetProperty("home").ValueKind == JsonValueKind.Null ? null : score.GetProperty("halftime").GetProperty("home").GetInt32(),
                        Team2 = score.GetProperty("halftime").GetProperty("away").ValueKind == JsonValueKind.Null ? null : score.GetProperty("halftime").GetProperty("away").GetInt32()
                    }
                };

                // Busca odds de pré-jogo
                partida.Odds = await ObterOddsPorJogo(partida.Id);
                partidas.Add(partida);
            }

            return partidas;
        }

        /// <summary>
        /// Busca odds de uma partida específica
        /// </summary>
        private async Task<OddsDto> ObterOddsPorJogo(int fixtureId)
        {
            var baseUrl = _config["Football:RAPIDAPI_FOOTBALL_URL"];
            string url = $"{baseUrl}/odds?fixture={fixtureId}";

            var response = await _httpClient.GetAsync(url);
            if (!response.IsSuccessStatusCode) return null;

            var json = await response.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(json);

            var odds = new OddsDto { Team1 = 0, Draw = 0, Team2 = 0 };

            var responseArray = doc.RootElement.GetProperty("response");
            if (responseArray.GetArrayLength() == 0) return odds;

            // Pega a primeira odd disponível (bookmaker padrão)
            var firstBookmaker = responseArray[0].GetProperty("bookmaker");

            foreach (var bet in firstBookmaker.GetProperty("bets").EnumerateArray())
            {
                if (bet.GetProperty("name").GetString() == "Match Winner")
                {
                    foreach (var value in bet.GetProperty("values").EnumerateArray())
                    {
                        var label = value.GetProperty("label").GetString()?.ToLower();
                        var oddValue = value.GetProperty("odd").GetDecimal();

                        if (label == "home") odds.Team1 = oddValue;
                        else if (label == "draw") odds.Draw = oddValue;
                        else if (label == "away") odds.Team2 = oddValue;
                    }
                }
            }

            return odds;
        }

        public async Task<FormattedMatchDto?> ObterJogoPorId(int gameId)
        {
            var match = await GetMatchById(gameId);
            if (match == null) return null;

            return new FormattedMatchDto
            {
                Id = match.Id,
                SportType = match.SportType,
                Competition = match.Competition,
                Team1 = match.Team1,
                Team2 = match.Team2,
                Team1Logo = match.Team1Logo,
                Team2Logo = match.Team2Logo,
                Date = match.Date,
                Time = match.Time,
                Status = match.Status,
                Elapsed = match.Elapsed,
                PlacarCasa = match.Score?.Team1,
                PlacarFora = match.Score?.Team2
            };
        }


        public async Task<FormattedMatch?> GetMatchById(int gameId)
        {
            var baseUrl = _config["Football:RAPIDAPI_FOOTBALL_URL"];
            var url = $"{baseUrl}/fixtures?id={gameId}";

            var response = await _httpClient.GetAsync(url);
            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(json);

            var item = doc.RootElement.GetProperty("response").EnumerateArray().FirstOrDefault();
            if (item.ValueKind == JsonValueKind.Undefined)
                return null;

            var fixture = item.GetProperty("fixture");
            var leagueObj = item.GetProperty("league");
            var teams = item.GetProperty("teams");
            var goals = item.GetProperty("goals");
            var score = item.GetProperty("score");

            var matchDate = DateTime.Parse(fixture.GetProperty("date").GetString() ?? DateTime.Now.ToString());

            return new FormattedMatch
            {
                Id = fixture.GetProperty("id").GetInt32(),
                Competition = leagueObj.GetProperty("name").GetString() ?? "",
                Team1 = teams.GetProperty("home").GetProperty("name").GetString() ?? "",
                Team2 = teams.GetProperty("away").GetProperty("name").GetString() ?? "",
                Team1Logo = teams.GetProperty("home").GetProperty("logo").GetString(),
                Team2Logo = teams.GetProperty("away").GetProperty("logo").GetString(),
                Date = matchDate.ToString("yyyy-MM-dd"),
                Time = matchDate.ToString("HH:mm"),
                Status = fixture.GetProperty("status").GetProperty("long").GetString() ?? "Desconhecido",
                Elapsed = fixture.GetProperty("status").TryGetProperty("elapsed", out var elapsed) ? elapsed.GetInt32() : null,
                Score = new ScoreResult
                {
                    Team1 = goals.GetProperty("home").ValueKind == JsonValueKind.Null ? null : goals.GetProperty("home").GetInt32(),
                    Team2 = goals.GetProperty("away").ValueKind == JsonValueKind.Null ? null : goals.GetProperty("away").GetInt32()
                },
                HalftimeScore = new ScoreResult
                {
                    Team1 = score.GetProperty("halftime").GetProperty("home").ValueKind == JsonValueKind.Null ? null : score.GetProperty("halftime").GetProperty("home").GetInt32(),
                    Team2 = score.GetProperty("halftime").GetProperty("away").ValueKind == JsonValueKind.Null ? null : score.GetProperty("halftime").GetProperty("away").GetInt32()
                },
                Odds = new Odds
                {
                    Team1 = 1,
                    Draw = 3,
                    Team2 = 3
                }
            };
        }


    }
}
