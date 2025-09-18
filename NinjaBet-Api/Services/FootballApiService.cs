using NinjaBet_Api.Models.Jogos;
using NinjaBet_Application.DTOs;
using NinjaBet_Application.Interfaces;
using System.Text.Json;

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

        public async Task<FormattedMatchDto?> ObterJogoPorId(int gameId)
        {
            var match = await GetMatchById(gameId);
            if (match == null) return null;

            return new FormattedMatchDto
            {
                Id = match.id,
                SportType = match.sportType,
                Competition = match.competition,
                Team1 = match.team1,
                Team2 = match.team2,
                Team1Logo = match.team1Logo,
                Team2Logo = match.team2Logo,
                Date = match.date,
                Time = match.time,
                Status = match.status,
                Elapsed = match.elapsed,
                PlacarCasa = match.score?.team1,
                PlacarFora = match.score?.team2
            };
        }

        public async Task<List<FormattedMatch>> GetMatches(string? date = null)
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

            var matches = new List<FormattedMatch>();

            foreach (var item in doc.RootElement.GetProperty("response").EnumerateArray())
            {
                var fixture = item.GetProperty("fixture");
                var leagueObj = item.GetProperty("league");
                var teams = item.GetProperty("teams");
                var goals = item.GetProperty("goals");
                var score = item.GetProperty("score");

                var matchDate = DateTime.Parse(fixture.GetProperty("date").GetString() ?? DateTime.Now.ToString());

                matches.Add(new FormattedMatch
                {
                    id = fixture.GetProperty("id").GetInt32(),
                    competition = leagueObj.GetProperty("name").GetString() ?? "",
                    team1 = teams.GetProperty("home").GetProperty("name").GetString() ?? "",
                    team2 = teams.GetProperty("away").GetProperty("name").GetString() ?? "",
                    team1Logo = teams.GetProperty("home").GetProperty("logo").GetString(),
                    team2Logo = teams.GetProperty("away").GetProperty("logo").GetString(),
                    date = matchDate.ToString("yyyy-MM-dd"),
                    time = matchDate.ToString("HH:mm"),
                    status = fixture.GetProperty("status").GetProperty("long").GetString() ?? "Desconhecido",
                    elapsed = fixture.GetProperty("status").TryGetProperty("elapsed", out var elapsed) ? elapsed.GetInt32() : null,
                    score = new ScoreResult
                    {
                        team1 = goals.GetProperty("home").ValueKind == JsonValueKind.Null ? null : goals.GetProperty("home").GetInt32(),
                        team2 = goals.GetProperty("away").ValueKind == JsonValueKind.Null ? null : goals.GetProperty("away").GetInt32()
                    },
                    halftimeScore = new ScoreResult
                    {
                        team1 = score.GetProperty("halftime").GetProperty("home").ValueKind == JsonValueKind.Null ? null : score.GetProperty("halftime").GetProperty("home").GetInt32(),
                        team2 = score.GetProperty("halftime").GetProperty("away").ValueKind == JsonValueKind.Null ? null : score.GetProperty("halftime").GetProperty("away").GetInt32()
                    },
                    odds = new Odds
                    {
                        team1 = 1.95,
                        draw = 3.40,
                        team2 = 3.80
                    }
                });
            }

            return matches;
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
                id = fixture.GetProperty("id").GetInt32(),
                competition = leagueObj.GetProperty("name").GetString() ?? "",
                team1 = teams.GetProperty("home").GetProperty("name").GetString() ?? "",
                team2 = teams.GetProperty("away").GetProperty("name").GetString() ?? "",
                team1Logo = teams.GetProperty("home").GetProperty("logo").GetString(),
                team2Logo = teams.GetProperty("away").GetProperty("logo").GetString(),
                date = matchDate.ToString("yyyy-MM-dd"),
                time = matchDate.ToString("HH:mm"),
                status = fixture.GetProperty("status").GetProperty("long").GetString() ?? "Desconhecido",
                elapsed = fixture.GetProperty("status").TryGetProperty("elapsed", out var elapsed) ? elapsed.GetInt32() : null,
                score = new ScoreResult
                {
                    team1 = goals.GetProperty("home").ValueKind == JsonValueKind.Null ? null : goals.GetProperty("home").GetInt32(),
                    team2 = goals.GetProperty("away").ValueKind == JsonValueKind.Null ? null : goals.GetProperty("away").GetInt32()
                },
                halftimeScore = new ScoreResult
                {
                    team1 = score.GetProperty("halftime").GetProperty("home").ValueKind == JsonValueKind.Null ? null : score.GetProperty("halftime").GetProperty("home").GetInt32(),
                    team2 = score.GetProperty("halftime").GetProperty("away").ValueKind == JsonValueKind.Null ? null : score.GetProperty("halftime").GetProperty("away").GetInt32()
                },
                odds = new Odds
                {
                    team1 = 1.95,
                    draw = 3.40,
                    team2 = 3.80
                }
            };
        }
    }
}
