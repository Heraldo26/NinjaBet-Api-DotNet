using NinjaBet_Api.Models.Jogos;
using NinjaBet_Application.Interfaces;
using NinjaBet_Dmain.Entities;
using System.Globalization;
using System.Text.Json;

namespace NinjaBet_Api.Services
{
    public class FootballApiService : IFootballApiService
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _config;

        private readonly List<League> _mainLeagues = new()
        {
            new League(39, "Premier League"),
            new League(140, "La Liga"),
            new League(135, "Serie A"),
            new League(2, "Champions League"),
            new League(2810, "Libertadores"),
            new League(71, "Brasileirão Serie A"),
            new League(72, "Brasileirão Serie B"),
            new League(73, "Brasileirão Serie C"),
            new League(74, "Brasileirão Serie D"),
            new League(2851, "Copa do Mundo de Clubes"),
            new League(1, "Copa do Mundo")
        };

        public FootballApiService(HttpClient httpClient, IConfiguration config)
        {
            _httpClient = httpClient;
            _config = config;

            _httpClient.DefaultRequestHeaders.Add("X-RapidAPI-Key", _config["Football:RAPIDAPI_KEY"]);
            _httpClient.DefaultRequestHeaders.Add("X-RapidAPI-Host", _config["Football:RAPIDAPI_HOST"]);
        }

        public List<League> GetMainLeagues() => _mainLeagues;

        public async Task<List<League>> GetMainLeaguesDynamicAsync()
        {
            var baseUrl = _config["Football:RAPIDAPI_FOOTBALL_URL"];
            var url = $"{baseUrl}/leagues";

            var response = await _httpClient.GetAsync(url);
            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(json);

            var responseArray = doc.RootElement.GetProperty("response");

            var ligasDesejadas = new[]
            {
                "La Liga",
                "Serie A",
                "CONMEBOL Libertadores",
                "Champions League",
                "Copa Do Brasil",
                "Brasileirão Serie A",
                "Brasileirão Serie B"
            };

            var leagues = new List<League>();

            foreach (var liga in responseArray.EnumerateArray())
            {
                var leagueName = liga.GetProperty("league").GetProperty("name").GetString();
                if (ligasDesejadas.Contains(leagueName))
                {
                    var currentSeason = liga.GetProperty("seasons")
                        .EnumerateArray()
                        .FirstOrDefault(s => s.GetProperty("current").GetBoolean());

                    if (currentSeason.ValueKind != JsonValueKind.Undefined)
                    {
                        leagues.Add(new League(
                            liga.GetProperty("league").GetProperty("id").GetInt32(),
                            leagueName
                        ));
                    }
                }
            }

            return leagues;

        }

        public async Task<List<Match>> GetJogosPorDataAsync(DateTime? data = null)
        {
            var baseUrl = _config["Football:RAPIDAPI_FOOTBALL_URL"];
            var date = (data ?? DateTime.UtcNow).ToString("yyyy-MM-dd"); // default: hoje (UTC)
            var url = $"{baseUrl}/fixtures?date={date}";

            return await ProcessarPartidasAsync(url);
        }

        public async Task<List<Match>> GetProximoJogos(int quantidade = 50)
        {
            var baseUrl = _config["Football:RAPIDAPI_FOOTBALL_URL"];
            var url = $"{baseUrl}/fixtures?next={quantidade}";
            return await ProcessarPartidasAsync(url);
        }

        public async Task<List<Match>> GetAoVivoAsync()
        {
            var baseUrl = _config["Football:RAPIDAPI_FOOTBALL_URL"];
            var url = $"{baseUrl}/fixtures?live=all";
            return await ProcessarPartidasAsync(url);
        }


        // Próximos jogos das ligas principais
        public async Task<List<Match>> GetPrincipaisCampeonatosAsync()
        {
            var baseUrl = _config["Football:RAPIDAPI_FOOTBALL_URL"];
            var season = _config["Football:RAPIDAPI_FOOTBALL_SEASON"];
            var partidas = new List<Match>();

            var leagues = await GetMainLeaguesDynamicAsync();

            foreach (var league in leagues)
            {
                var url = $"{baseUrl}/fixtures?league={league.Id}&season={season}&next=10";
                var leagueMatches = await ProcessarPartidasAsync(url);
                partidas.AddRange(leagueMatches);
            }

            return partidas;
        }

        // Jogos de uma liga específica
        public async Task<List<Match>> GetJogosDaLigaAsync(int leagueId)
        {
            var baseUrl = _config["Football:RAPIDAPI_FOOTBALL_URL"];
            var season = _config["Football:RAPIDAPI_FOOTBALL_SEASON"];
            var url = $"{baseUrl}/fixtures?league={leagueId}&season={season}&next=20";
            return await ProcessarPartidasAsync(url);
        }

        /// <summary>
        /// Método interno que processa as partidas e chama odds
        /// </summary>
        private async Task<List<Match>> ProcessarPartidasAsync(string url)
        {
            try
            {
                var response = await _httpClient.GetAsync(url);
                response.EnsureSuccessStatusCode();

                var json = await response.Content.ReadAsStringAsync();
                using var doc = JsonDocument.Parse(json);

                var partidas = new List<Match>();

                foreach (var item in doc.RootElement.GetProperty("response").EnumerateArray())
                {
                    var fixture = item.GetProperty("fixture");
                    var statusShort = fixture.GetProperty("status").GetProperty("short").GetString();
                    var leagueObj = item.GetProperty("league");
                    var teams = item.GetProperty("teams");
                    var goals = item.GetProperty("goals");
                    var score = item.GetProperty("score");

                    // Pula jogos que já finalizaram
                    if (statusShort == "FT" || statusShort == "AET" || statusShort == "PEN")
                        continue;

                    var matchDate = DateTime.Parse(fixture.GetProperty("date").GetString() ?? DateTime.Now.ToString());
                    var fixtureId = fixture.GetProperty("id").GetInt32();

                    var odds = await ObterOddsDaPartidaAsync(fixtureId);

                    var match = new Match
                    {
                        Id = fixtureId,
                        Competition = leagueObj.GetProperty("name").GetString() ?? "",
                        Team1 = teams.GetProperty("home").GetProperty("name").GetString() ?? "",
                        Team2 = teams.GetProperty("away").GetProperty("name").GetString() ?? "",
                        Team1Logo = teams.GetProperty("home").GetProperty("logo").GetString(),
                        Team2Logo = teams.GetProperty("away").GetProperty("logo").GetString(),
                        Date = matchDate,
                        Status = fixture.GetProperty("status").GetProperty("long").GetString() ?? "Desconhecido",
                        Elapsed = fixture.GetProperty("status").TryGetProperty("elapsed", out var elapsed) && elapsed.ValueKind == JsonValueKind.Number
                                  ? elapsed.GetInt32()
                                  : (int?)null,
                        Score = new ScoreResult
                        {
                            Team1 = goals.TryGetProperty("home", out var gHome) && gHome.ValueKind == JsonValueKind.Number ? gHome.GetInt32() : (int?)null,
                            Team2 = goals.TryGetProperty("away", out var gAway) && gAway.ValueKind == JsonValueKind.Number ? gAway.GetInt32() : (int?)null
                        },
                        HalftimeScore = new ScoreResult
                        {
                            Team1 = score.TryGetProperty("halftime", out var ht) && ht.TryGetProperty("home", out var htHome) && htHome.ValueKind == JsonValueKind.Number
                                  ? htHome.GetInt32()
                                  : (int?)null,
                            Team2 = score.TryGetProperty("halftime", out var ht2) && ht2.TryGetProperty("away", out var htAway) && htAway.ValueKind == JsonValueKind.Number
                                  ? htAway.GetInt32()
                                  : (int?)null
                        },
                        Odds = odds
                    };

                    partidas.Add(match);
                }

                return partidas;
            }
            catch (Exception ex)
            {
                throw new Exception($"Erro ao obter partidas: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Busca as odds de uma partida específica usando o endpoint de odds
        /// </summary>
        private async Task<Odds> ObterOddsDaPartidaAsync(int fixtureId)
        {
            var baseUrl = _config["Football:RAPIDAPI_FOOTBALL_URL"];
            var url = $"{baseUrl}/odds?fixture={fixtureId}";

            var response = await _httpClient.GetAsync(url);
            if (!response.IsSuccessStatusCode)
                return new Odds(0, 0, 0);

            var json = await response.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(json);

            var responseArray = doc.RootElement.GetProperty("response");
            if (responseArray.GetArrayLength() == 0)
                return new Odds(0, 0, 0);

            var bookmaker = responseArray[0].GetProperty("bookmakers")[0];
            var bets = bookmaker.GetProperty("bets");

            var h2hBet = bets.EnumerateArray().FirstOrDefault(b => b.GetProperty("name").GetString() == "Match Winner");
            if (h2hBet.ValueKind == JsonValueKind.Undefined)
                return new Odds(0, 0, 0);

            var values = h2hBet.GetProperty("values");

            decimal team1 = 0, draw = 0, team2 = 0;
            foreach (var v in values.EnumerateArray())
            {
                var type = v.GetProperty("value").GetString();
                var odd = decimal.Parse(v.GetProperty("odd").GetString(), CultureInfo.InvariantCulture);

                if (type == "Home") team1 = odd;
                else if (type == "Draw") draw = odd;
                else if (type == "Away") team2 = odd;
            }

            return new Odds(team1, draw, team2);
        }


        public async Task<Match?> ObterJogoPorId(int gameId)
        {
            var baseUrl = _config["Football:RAPIDAPI_FOOTBALL_URL"];
            var url = $"{baseUrl}/fixtures?id={gameId}";

            var response = await _httpClient.GetAsync(url);
            if (!response.IsSuccessStatusCode) return null;

            var json = await response.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(json);

            var item = doc.RootElement.GetProperty("response").EnumerateArray().FirstOrDefault();
            if (item.ValueKind == JsonValueKind.Undefined) return null;

            var fixture = item.GetProperty("fixture");
            var leagueObj = item.GetProperty("league");
            var teams = item.GetProperty("teams");
            var goals = item.GetProperty("goals");
            var score = item.GetProperty("score");

            var matchDate = DateTime.Parse(fixture.GetProperty("date").GetString() ?? DateTime.Now.ToString());

            // Opcional: buscar odds também, se quiser mostrar no bilhete
            var odds = await ObterOddsDaPartidaAsync(fixture.GetProperty("id").GetInt32());

            return new Match
            {
                Id = fixture.GetProperty("id").GetInt32(),
                Competition = leagueObj.GetProperty("name").GetString() ?? "",
                Team1 = teams.GetProperty("home").GetProperty("name").GetString() ?? "",
                Team2 = teams.GetProperty("away").GetProperty("name").GetString() ?? "",
                Team1Logo = teams.GetProperty("home").GetProperty("logo").GetString(),
                Team2Logo = teams.GetProperty("away").GetProperty("logo").GetString(),
                Date = matchDate,
                Status = fixture.GetProperty("status").GetProperty("long").GetString() ?? "Desconhecido",
                Score = new ScoreResult
                {
                    Team1 = goals.GetProperty("home").ValueKind == JsonValueKind.Null ? null : goals.GetProperty("home").GetInt32(),
                    Team2 = goals.GetProperty("away").ValueKind == JsonValueKind.Null ? null : goals.GetProperty("away").GetInt32()
                },
                Odds = odds
            };
        }

    }
}
