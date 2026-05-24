using HtmlAgilityPack;
using NuLigaViewer.Data;
using System.Collections.Concurrent;
using System.Globalization;

namespace NuLigaViewer
{
    public static class NuLigaParser
    {
        private static readonly string urlRoot = "https://bsv-schach.liga.nu/";
        private static readonly HtmlWeb web = new();
        private static readonly ConcurrentDictionary<string, HtmlNodeCollection?> _cachedTeamPages = new();
        private static readonly ConcurrentDictionary<string, Tuple<string?, Dictionary<string, DewisClubPlayer>?>> _cachedDewisClubPlayers = new();
        private static readonly ConcurrentDictionary<string, List<ClubPlayer>> _cachedClubPlayers = new();
        private static readonly ConcurrentDictionary<string, Dictionary<string, int>> TeamToClubPlayerToDwzMapping = new();

        private static readonly CultureInfo _nuLigaCulture = new("de-DE");

        public static event Action<League, TeamPairing>? TeamPairingReportLoadedForGui;

        public static List<BadenRegion> ParseLeagues()
        {
            var regions = new List<BadenRegion>();
            var badenLeagues = new List<string>
            {
                "Baden",
                "Mannheim",
                "Heidelberg",
                "Karlsruhe",
                "Pforzheim",
                "Mittelbaden",
                "Ortenau",
                "Odenwald",
                "Freiburg",
                "Hochrhein",
                "Schwarzwald",
                "Bodensee",
            };

            foreach (var region in badenLeagues)
            {
                try
                {
                    var url = $"{urlRoot}cgi-bin/WebObjects/nuLigaSCHACHDE.woa/wa/leaguePage?championship={region}+25%2F26";
                    regions.Add(new BadenRegion(name: region, leagues: ParseLeaguesFromUrl(web, url)));
                }
                catch (Exception e)
                {
                    System.Diagnostics.Debug.WriteLine(e.ToString());
                }
            }

            return regions;
        }

        private static List<League> ParseLeaguesFromUrl(HtmlWeb web, string url)
        {
            var doc = web.Load(url);
            var crossTableList = doc.DocumentNode.SelectNodes("//table[@class='matrix']");
            if (crossTableList == null || crossTableList.Count < 1)
            {
                return [];
            }

            var leagueList = crossTableList[0];
            var leagues = new List<League>();
            var rows = leagueList.SelectNodes(".//a[starts-with(@href, '/cgi')]");
            for (var row = 0; row < rows.Count; row++)
            {
                var league = new League
                {
                    Name = rows[row].InnerText.Trim('\n', '\t', ' '),
                    Url = urlRoot + rows[row].Attributes["href"].Value.TrimStart('/').Replace("amp;", ""),
                };
                leagues.Add(league);
            }

            return leagues;
        }

        public static async Task<Team[]> ParseTeams(League league)
        {
            var doc = web.Load(league.Url);
            var crossTableList = doc.DocumentNode.SelectNodes("//table[@class='cross-table']");
            if (crossTableList == null || crossTableList.Count < 1)
            {
                return [];
            }

            var rankingTable = crossTableList[0];
            var rows = rankingTable.SelectNodes("tr");

            // start with 1, skip headers in 0
            await Parallel.ForAsync(1, rows.Count, async (row, ct) =>
            {
                var cells = rows[row].SelectNodes("th|td");
                var teamName = cells[2].InnerText;
                var teamUrl = cells[2].QuerySelector("a").Attributes["href"].Value.TrimStart('/').Replace("amp;", "");
                if (string.IsNullOrEmpty(teamUrl))
                {
                    return;
                }
                teamUrl = urlRoot + teamUrl;

                if (!_cachedTeamPages.TryGetValue(teamUrl, out var resultSetList))
                {
                    resultSetList = TryLoadWebResourceThreeTimes(web, teamUrl, "//table[@class='result-set']");
                    _cachedTeamPages.TryAdd(teamUrl, resultSetList);
                }
                if (resultSetList != null && resultSetList.Count >= 1)
                {
                    var clubUrl = urlRoot + resultSetList[0].SelectNodes("tr")[0].SelectNodes("th|td")[1].QuerySelector("a").Attributes["href"].Value.TrimStart('/').Replace("amp;", "");
                    (_, var dewisClubPlayers) = await LoadClubResources(clubUrl);
                    if (dewisClubPlayers != null && !TeamToClubPlayerToDwzMapping.ContainsKey(teamName))
                    {
                        Dictionary<string, int> playerToDwzMapping = new();
                        foreach (var kv in dewisClubPlayers)
                        {
                            var playerKey = kv.Key;
                            if (kv.Value.DWZ != null)
                            {
                                playerToDwzMapping[playerKey] = kv.Value.DWZ.Value;
                            }
                        }
                        TeamToClubPlayerToDwzMapping.TryAdd(teamName, playerToDwzMapping);
                    }
                }
            });

            var numberOfTeams = rows.Count - 1; // BW Liga has 12 teams, others 10, KKC has 8 (+ header)
            var teams = new Team[numberOfTeams];

            // start with 1, skip headers in 0
            Parallel.For(1, rows.Count, row =>
            {
                var cells = rows[row].SelectNodes("th|td");
                var newTeam = ParseTeam(cells, row, numberOfTeams, league);
                teams[row - 1] = newTeam;
            });
            return teams;
        }

        private static Team ParseTeam(HtmlNodeCollection cells, int row, int numberOfTeams, League league)
        {
            var teamUrl = cells[2].QuerySelector("a").Attributes["href"].Value.TrimStart('/').Replace("amp;", "");

            var newTeam = new Team
            {
                Qualification = QualificationHelper.ParseQualification(cells[0]?.QuerySelector("img")?.Attributes["alt"]?.Value ?? string.Empty),
                Rang = int.Parse(cells[1].InnerText),
                Name = cells[2].InnerText,
                TeamUrl = string.IsNullOrEmpty(teamUrl) ? null : urlRoot + teamUrl,
                Spiele = int.Parse(cells[numberOfTeams + 3].InnerText),
                Punkte = int.Parse(cells[numberOfTeams + 4].InnerText),
                BP = double.Parse(cells[numberOfTeams + 5].InnerText, NumberStyles.Any, _nuLigaCulture),
                BoardPointsPerRank = new double[numberOfTeams - 1]
            };

            ParseGameDaysAndPlayers(newTeam, numberOfTeams, league);

            var rankIndex = 0;
            for (var i = 0; i < numberOfTeams; i++)
            {
                if (row == i + 1)
                {
                    continue;
                }
                var value = string.IsNullOrEmpty(cells[3 + i].InnerText) ? "0" : cells[3 + i].InnerText;
                newTeam.BoardPointsPerRank[rankIndex] = double.TryParse(value, NumberStyles.Any, _nuLigaCulture, out var result) ? result : 0;
                rankIndex++;
            }

            return newTeam;
        }

        private static async void ParseGameDaysAndPlayers(Team newTeam, int numberOfTeams, League league)
        {
            if (newTeam.TeamUrl == null)
            {
                return;
            }

            if (!_cachedTeamPages.TryGetValue(newTeam.TeamUrl, out var resultSetList))
            {
                // should not be reached:
                resultSetList = TryLoadWebResourceThreeTimes(web, newTeam.TeamUrl, "//table[@class='result-set']");
                _cachedTeamPages.TryAdd(newTeam.TeamUrl, resultSetList);
            }
            if (resultSetList != null && resultSetList.Count >= 1)
            {
                var clubUrl = urlRoot + resultSetList[0].SelectNodes("tr")[0].SelectNodes("th|td")[1].QuerySelector("a").Attributes["href"].Value.TrimStart('/').Replace("amp;", "");

                (string? clubLineUpsUrl, var dewisClubPlayers) = await LoadClubResources(clubUrl);
                newTeam.ClubLineUpsUrl = clubLineUpsUrl;
                newTeam.ClubPlayers = dewisClubPlayers;
            }

            newTeam.GameDays = ParseGameDays(resultSetList, newTeam.GameDayReportLoaded, league);
            newTeam.TeamPlayers = ParsePlayers(resultSetList, newTeam, newTeam.GameDays?.Count ?? numberOfTeams - 1);
        }

        private async static Task<Tuple<string?, Dictionary<string, DewisClubPlayer>?>> LoadClubResources(string clubUrl)
        {
            if (_cachedDewisClubPlayers.TryGetValue(clubUrl, out var cachedClubs))
            {
                return cachedClubs;
            }

            try
            {
                var clubTables = TryLoadWebResourceThreeTimes(web, clubUrl, "//table");
                if (clubTables == null || clubTables.Count < 1)
                {
                    return new Tuple<string?, Dictionary<string, DewisClubPlayer>?>(null, null);
                }

                var clubPageLinks = clubTables[0].SelectNodes("tr")[0].SelectNodes("th|td")[0].SelectNodes("ul")[0].SelectNodes("li");
                var clubLineUpsUrl = urlRoot + clubPageLinks[2].QuerySelector("a").Attributes["href"].Value.TrimStart('/').Replace("amp;", "");

                var pLine = clubTables[0].SelectNodes("tr")[0].SelectNodes("th|td")[0].SelectNodes("p")[0].InnerText;
                var pLineTrimmed = pLine.Replace("&nbsp;", "").Trim().Trim('\r', '\n').Trim();
                var vnrNumber = pLineTrimmed.Split(',')[0];
                var zpsNumber = vnrNumber.Substring(vnrNumber.IndexOf(':') + 1) ?? "";

                var clubPlayers = await DewisAccess.GetClubPlayers(zpsNumber);
                var clubResource = new Tuple<string?, Dictionary<string, DewisClubPlayer>?>(clubLineUpsUrl, clubPlayers);
                _cachedDewisClubPlayers.TryAdd(clubUrl, clubResource);

                return clubResource;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Failed to load Dewis club players for club {clubUrl}: {ex.Message}");
            }

            return new Tuple<string?, Dictionary<string, DewisClubPlayer>?>(null, null);
        }

        private static List<TeamPairing>? ParseGameDays(HtmlNodeCollection? resultSetList, Action<TeamPairing> gameDayReportLoaded, League league)
        {
            if (resultSetList == null || resultSetList.Count < 2)
            {
                var errorReason = resultSetList == null ? "resultSetList is null" : "resultSetList's count < 2";
                System.Diagnostics.Debug.WriteLine($"Error in loaded data for game days: {errorReason}");
                return null;
            }

            var gameDays = new List<TeamPairing>();
            var gameDayTable = resultSetList[1];

            var rows = gameDayTable.SelectNodes("tr");
            for (var row = 1; row < rows.Count; row++)
            {
                var cells = rows[row].SelectNodes("th|td");
                var date = cells[1].InnerText.Trim('\n', '\t', ' ');
                var round = cells[4].InnerText.Trim('\n', '\t', ' ');
                var homeTeam = cells[6].InnerText.Trim('\n', '\t', ' ');
                var guestTeam = cells[7].InnerText.Trim('\n', '\t', ' ').Replace("&nbsp;", "");
                var boardPoints = cells[8].InnerText.Trim('\n', '\t', ' ');
                var reportUrl = cells[8].QuerySelector("a")?.Attributes["href"].Value.TrimStart('/').Replace("amp;", "");

                var gameDay = new TeamPairing
                {
                    Datum = DateTime.ParseExact(date, "d", new CultureInfo("de-DE")),
                    RoundByCount = row,
                    Runde = int.Parse(round),
                    HeimMannschaft = homeTeam,
                    GastMannschaft = guestTeam,
                    BrettPunkte = boardPoints,
                    ReportUrl = string.IsNullOrEmpty(reportUrl) ? null : urlRoot + reportUrl
                };

                gameDays.Add(gameDay);

                _ = LoadGameReportAsync(gameDay, gameDayReportLoaded, league);
            }

            return gameDays;
        }

        private static async Task LoadGameReportAsync(TeamPairing? teamPairing, Action<TeamPairing> gameDayReportLoaded, League league)
        {
            if (teamPairing == null || string.IsNullOrWhiteSpace(teamPairing.ReportUrl))
            {
                return;
            }

            await Task.Run(() =>
            {
                var web = new HtmlWeb();
                var resultSetList = TryLoadWebResourceThreeTimes(web, teamPairing.ReportUrl, "//table[@class='result-set']");
                teamPairing.Report = ParseGameReport(resultSetList, teamPairing);

                gameDayReportLoaded(teamPairing);
                TeamPairingReportLoadedForGui?.Invoke(league, teamPairing);
            });
        }

        private static GameReport? ParseGameReport(HtmlNodeCollection? resultSetList, TeamPairing teamPairing)
        {
            if (resultSetList == null || resultSetList.Count < 1)
            {
                var errorReason = resultSetList == null ? "resultSetList is null" : "resultSetList's count < 1";
                System.Diagnostics.Debug.WriteLine($"Error in loaded data for the game report: {errorReason}");
                return null;
            }

            var pairings = new List<Pairing>();
            var gameReportTable = resultSetList[0];
            var homeClubPlayerToDwzMapping = TeamToClubPlayerToDwzMapping.GetValueOrDefault(teamPairing.HeimMannschaft ?? "");
            var guestClubPlayerToDwzMapping = TeamToClubPlayerToDwzMapping.GetValueOrDefault(teamPairing.GastMannschaft ?? "");

            var rows = gameReportTable.SelectNodes("tr");
            for (var row = 1; row < rows.Count; row++)
            {
                var cells = rows[row].SelectNodes("th|td");
                if (cells.Count < 6)
                {
                    continue;
                }
                var homePlayerName = cells[1].InnerText.Trim('\n', '\t', ' ');
                var guestPlayerName = cells[3].InnerText.Trim('\n', '\t', ' ');
                var homePlayerDWZ = cells[2].InnerText.Trim('\n', '\t', ' ');
                var guestPlayerDWZ = cells[4].InnerText.Trim('\n', '\t', ' ');

                var pairing = new Pairing
                {
                    Brett = int.Parse(cells[0].InnerText.Trim('\n', '\t', ' ')),
                    HeimSpieler = homePlayerName,
                    HeimSpielerDWZ = int.Parse(string.IsNullOrEmpty(homePlayerDWZ) ? "1000" : homePlayerDWZ),
                    GastSpieler = guestPlayerName,
                    GastSpielerDWZ = int.Parse(string.IsNullOrEmpty(guestPlayerDWZ) ? "1000" : guestPlayerDWZ),
                    Ergebnis = cells[5].InnerText.Trim('\n', '\t', ' '),
                    RelatedTeamPairing = teamPairing
                };

                if (homeClubPlayerToDwzMapping?.TryGetValue(homePlayerName, out var dewisDwzForHomePlayer) == true)
                {
                    pairing.HeimSpielerDWZ = dewisDwzForHomePlayer;
                }
                if (guestClubPlayerToDwzMapping?.TryGetValue(guestPlayerName, out var dewisDwzForGuestPlayer) == true)
                {
                    pairing.GastSpielerDWZ = dewisDwzForGuestPlayer;
                }

                pairings.Add(pairing);
            }

            return new GameReport { Pairings = pairings };
        }

        private static List<Player>? ParsePlayers(HtmlNodeCollection? resultSetList, Team newTeam, int numberOfGameDays)
        {
            if (resultSetList == null || resultSetList.Count < 3)
            {
                var errorReason = resultSetList == null ? "resultSetList is null" : "resultSetList's count < 3";
                System.Diagnostics.Debug.WriteLine($"Error in loaded data for the players: {errorReason}");
                return null;
            }

            var players = new List<Player>();
            var playerTable = resultSetList[2];

            var rows = playerTable.SelectNodes("tr");
            for (var row = 0; row < rows.Count; row++)
            {
                var cells = rows[row].SelectNodes("th|td");
                if (cells.Count < 6 || cells[0].InnerText == "Brett" || int.Parse(cells[4].InnerText) < 1)
                {
                    continue;
                }

                var player = new Player
                {
                    Brett = int.Parse(cells[0].InnerText),
                    Name = cells[1].InnerText.Trim('\n', '\t', ' '),
                    DWZ = int.Parse(string.IsNullOrEmpty(cells[3].InnerText) ? "1000" : cells[3].InnerText),
                    Games = int.Parse(cells[4].InnerText),
                    BoardPoints = cells[5].InnerText.Trim('\n', '\t', ' '),
                    TeamName = newTeam.Name,
                    PlayerInfoPerGameDay = new PlayerGameDayInfo[numberOfGameDays]
                };


                if (newTeam.ClubPlayers?.TryGetValue(player.Name, out DewisClubPlayer? dewisPlayer) ?? false)
                {
                    player.DWZ = dewisPlayer?.DWZ ?? 1000;
                }
                players.Add(player);
            }

            return players;
        }

        private static HtmlNodeCollection? TryLoadWebResourceThreeTimes(HtmlWeb web, string url, string nodeRequest)
        {
            for (var attempt = 1; attempt <= 3; attempt++)
            {
                try
                {
#if DEBUG
                    var html = InternetFileCache.Instance.Get(url);
                    var doc = new HtmlDocument();
                    doc.LoadHtml(html);
#else
                    var doc = web.Load(url);
#endif

                    var resultSetList = doc.DocumentNode.SelectNodes(nodeRequest);
                    if (resultSetList != null)
                    {
                        return resultSetList;
                    }
                    Thread.Sleep(10);
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Attempt {attempt} to load {url} failed: {ex.Message}");
                }
            }

            return null;
        }

        public static List<ClubPlayer> ParseAllClubPlayers(string? clubLineUpsUrl, string teamName)
        {
            if (string.IsNullOrEmpty(clubLineUpsUrl))
            {
                return [];
            }

            if (_cachedClubPlayers.TryGetValue(clubLineUpsUrl, out var cachedClubsPlayers))
            {
                return cachedClubsPlayers;
            }

            return LoadClubLineUpResource(clubLineUpsUrl, teamName);
        }

        private static List<ClubPlayer> LoadClubLineUpResource(string clubLineUpsUrl, string teamName)
        {
            try
            {
                var clubLineUpsPage = TryLoadWebResourceThreeTimes(web, clubLineUpsUrl, "//table[@class='result-set']");
                var lineUps = clubLineUpsPage?[0].SelectNodes("tr");
                if (lineUps == null)
                {
                    return [];
                }

                var lineUpUrl = string.Empty;
                for (var lineUpIndex = 0; lineUpIndex < lineUps.Count; lineUpIndex++)
                {
                    var cells = lineUps[lineUpIndex].SelectNodes("th|td");

                    var lineUpFound = cells.Any(x => x.InnerHtml.Contains("Punktspielbetrieb")
                                            && x.InnerHtml.Contains("2025/26")
                                            && !x.InnerHtml.Contains("Senioren")
                                            && !x.InnerHtml.Contains("Jugend")
                                            && !x.InnerHtml.Contains("Pokal"));
                    if (lineUpFound && lineUpIndex + 2 < lineUps.Count)
                    {
                        var lineUpRoute = lineUps[lineUpIndex + 2].SelectNodes("th|td")[1].QuerySelector("a").Attributes["href"].Value.Replace("&amp;", "&");
                        lineUpUrl = urlRoot + lineUpRoute.TrimStart('/');
                        break;
                    }
                }

                var lineUpPage = TryLoadWebResourceThreeTimes(web, lineUpUrl, "//table[@class='result-set']");
                var playerRows = lineUpPage?[0].SelectNodes("tr");
                var clubPlayers = new List<ClubPlayer>();

                // start with 1, skip headers in 0
                for (var row = 1; row < playerRows?.Count; row++)
                {
                    var cells = playerRows[row].SelectNodes("th|td");
                    if (cells.Count < 6 || cells[1].InnerText == "DWZ")
                    {
                        continue;
                    }

                    var dwz = int.TryParse(cells[1].InnerText, out var parsedDWZ) ? parsedDWZ : (int?)null;
                    var number = int.TryParse(cells[3].InnerText, out var parsedNumber) ? parsedNumber : (int?)null;
                    var playerUrl = cells[2].QuerySelector("a")?.Attributes["href"].Value.TrimStart('/').Replace("amp;", "");
                    var clubPlayerName = cells[2].InnerText.Trim('\n', '\t', ' ');

                    var player = new ClubPlayer
                    {
                        Rang = int.Parse(cells[0].InnerText),
                        DWZ = dwz,
                        Name = clubPlayerName,
                        Number = number,
                        Status = cells[5].InnerText.Trim('\n', '\t', ' '),
                        Url = string.IsNullOrEmpty(playerUrl) ? null : urlRoot + playerUrl
                    };
                    if (TeamToClubPlayerToDwzMapping.GetValueOrDefault(teamName)?.TryGetValue(clubPlayerName, out var dewisDWZ) == true)
                    {
                        player.DWZ = dewisDWZ;
                    }
                    clubPlayers.Add(player);
                }

                _cachedClubPlayers.TryAdd(clubLineUpsUrl, clubPlayers);
                return clubPlayers;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Failed to load club line-up resource for URL {clubLineUpsUrl}: {ex.Message}");
            }

            return [];
        }

        public static Player? ParseClubPlayerDetails(string playerUrl, string name, string? teamName)
        {
            var tables = TryLoadWebResourceThreeTimes(web, playerUrl, "//table[@class='result-set']");
            if (tables == null || tables.Count < 1)
            {
                return null;
            }

            var playerInfoTable = tables[0];
            var dwzString = playerInfoTable.SelectNodes("tr")[4].SelectNodes("th|td")[1].InnerText.Trim('\n', '\t', ' ');

            var playerDetails = new Player
            {
                Name = name,
                DWZ = int.TryParse(dwzString, out var parsedDWZ) ? parsedDWZ : 1000,
            };

            if (TeamToClubPlayerToDwzMapping.GetValueOrDefault(teamName ?? "")?.TryGetValue(name, out var dewisDWZ) == true)
            {
                playerDetails.DWZ = dewisDWZ;
            }

            if (tables.Count > 1)
            {
                var pairingTable = tables[1];
                var pairings = new List<Pairing>();
                var secondPairings = new List<Pairing>();

                var rows = pairingTable.SelectNodes("tr");
                for (var row = 0; row < rows.Count; row++)
                {
                    var cells = rows[row].SelectNodes("th|td");
                    if (cells.Count < 7 || cells[0].InnerText == "Datum")
                    {
                        continue;
                    }

                    var guestPlayerDWZ = cells[3].InnerText.Trim('\n', '\t', ' ');
                    var opponentTeamName = cells[5].InnerText.Trim('\n', '\t', ' ');
                    var opponentUrl = cells[2].QuerySelector("a")?.Attributes["href"].Value.TrimStart('/').Replace("amp;", "");
                    var opponentName = cells[2].InnerText.Split('(')[0].Trim('\n', '\t', ' ');

                    var dummyTp = new TeamPairing
                    {
                        HeimMannschaft = "Dummy",
                        GastMannschaft = opponentTeamName,
                        Datum = DateTime.TryParseExact(cells[0].InnerText.Trim('\n', '\t', ' '), "d", new CultureInfo("de-DE"), DateTimeStyles.None, out var dateTime) ? dateTime : DateTime.Today,
                    };

                    var pairing = new Pairing
                    {
                        Brett = int.Parse(cells[1].InnerText.Trim('\n', '\t', ' ')),
                        HeimSpieler = name,
                        HeimSpielerDWZ = playerDetails.DWZ,
                        GastSpieler = opponentName,
                        GastSpielerDWZ = int.TryParse(guestPlayerDWZ, out var parsedGuestDWZ) ? parsedGuestDWZ : 1000,
                        OpponentUrl = string.IsNullOrEmpty(opponentUrl) ? null : urlRoot + opponentUrl,
                        Ergebnis = cells[4].InnerText.Trim('\n', '\t', ' '),
                        RelatedTeamPairing = dummyTp
                    };

                    if (TeamToClubPlayerToDwzMapping.GetValueOrDefault(opponentTeamName)?.TryGetValue(name, out var opponentDewisDWZ) == true)
                    {
                        pairing.GastSpielerDWZ = opponentDewisDWZ;
                    }

                    // If date is empty, it is a second pairing for the same gameday.
                    if (string.IsNullOrEmpty(cells[0].InnerText.Replace("&nbsp;", "")))
                    {
                        if (row > 0)
                        {
                            var lastPairing = pairings.Last();
                            pairing.RelatedTeamPairing.GastMannschaft = lastPairing.RelatedTeamPairing?.GastMannschaft;
                            pairing.RelatedTeamPairing.Datum = lastPairing.RelatedTeamPairing!.Datum;
                            secondPairings.Add(pairing);
                        }
                    }
                    else
                    {
                        pairings.Add(pairing);
                    }
                }

                playerDetails.PlayerInfoPerGameDay = new PlayerGameDayInfo[pairings.Count];

                var index = 0;
                foreach (var pairing in pairings)
                {
                    playerDetails.PlayerInfoPerGameDay[index] = new PlayerGameDayInfo
                    {
                        Pairing = pairing,
                        PlayerIsInHomeTeam = true,
                    };

                    var secondPairing = secondPairings.FirstOrDefault(x => x.RelatedTeamPairing!.Datum == pairing.RelatedTeamPairing!.Datum);
                    if (secondPairing != null)
                    {
                        playerDetails.PlayerInfoPerGameDay[index]!.SecondPairing = secondPairing;
                    }
                    index++;
                }
            }

            return playerDetails;
        }
    }
}
