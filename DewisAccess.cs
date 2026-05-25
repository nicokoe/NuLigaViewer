using NuLigaViewer.Data;

namespace NuLigaViewer
{
    public static class DewisAccess
    {
        public static async Task<Dictionary<int, DewisClubPlayer>?> GetClubPlayers(string zpsNumber)
        {
            var players = new Dictionary<int, DewisClubPlayer>();
            var url = $"https://www.schachbund.de/php/dewis/verein.php?zps={zpsNumber}&format=csv";

            using (HttpClient client = new())
            {
#if DEBUG
                var content = await InternetFileCache.Instance.GetAsync(url);
#else
                var content = await client.GetStringAsync(url);
#endif

                if (content != null)
                {
                    var contentLines = content.Split(Environment.NewLine);

                    foreach (var line in contentLines)
                    {
                        if (line.StartsWith("id|nachname|vorname|titel|verein|mglnr|status|dwz|dwzindex|turniercode|turnierende|fideid|fideelo|fidetitel"))
                        {
                            continue;
                        }
                        var splittedEntries = line.Split('|');
                        if (splittedEntries.Length != 14)
                        {
                            continue;
                        }

                        var nn = splittedEntries[1];
                        var vn = splittedEntries[2];

                        int memberNumber = int.Parse(splittedEntries[5]);
                        players[memberNumber] = new DewisClubPlayer
                        {
                            Pkz = int.TryParse(splittedEntries[0], out var playerId) ? playerId : null,
                            Nachname = nn,
                            Vorname = vn,
                            Titel = splittedEntries[3],
                            DWZ = int.TryParse(splittedEntries[7], out var dwz) ? dwz : null,
                        };
                    }
                }
            }

            return players;
        }

        public static async Task<DewisClubPlayerLeagueDetails?> GetClubPlayerLeaguePerformance(string? pkzNumber, League? league)
        {
            if (pkzNumber == null || league == null)
            {
                return null;
            }

            var url = $"https://www.schachbund.de/php/dewis/spieler.php?pkz={pkzNumber}&format=csv";

            using (HttpClient client = new())
            {
                var content = await client.GetStringAsync(url);

                if (content != null)
                {
                    var contentLines = content.Split(Environment.NewLine);
                    if (league.Name.Contains("BWL"))
                    {
                        var bwLeagueLine = contentLines.LastOrDefault(line => line.Contains("BW-Liga"));
                        if (bwLeagueLine == null)
                        {
                            return null;
                        }

                        return ParseClubPlayerLeagueDetails(bwLeagueLine.Split('|'));
                    }

                    var skipUntilTournamentLines = true;
                    foreach (var line in contentLines)
                    {
                        if (skipUntilTournamentLines)
                        {
                            if (line.StartsWith("turniercode|turniername|dwzalt|dwzaltindex|punkte|partien|nichtgewertet|erwartungswert|gegner|koeffizient|dwzneu|dwzneuindex|leistung"))
                            {
                                skipUntilTournamentLines = false;
                            }
                            continue; // skip until we reach the tournament lines
                        }
                        var splittedEntries = line.Split('|');
                        if (splittedEntries.Length != 13)
                        {
                            continue;
                        }

                        var tournamentName = splittedEntries[1];
                        if (!TournamentNameMatchesExpectedLeague(tournamentName, league))
                        {
                            continue;
                        }

                        return ParseClubPlayerLeagueDetails(splittedEntries);
                    }
                }
            }

            return null;
        }

        private static DewisClubPlayerLeagueDetails ParseClubPlayerLeagueDetails(string[] splittedEntries)
        {
            int? dwzOld = int.TryParse(splittedEntries[2], out var dwz) ? dwz : null;
            int? opponentDwz = int.TryParse(splittedEntries[8], out var opponentDwzValue) ? opponentDwzValue : null;
            int? dwzNew = int.TryParse(splittedEntries[10], out var dwzNewValue) ? dwzNewValue : null;
            int? performance = int.TryParse(splittedEntries[12], out var performanceValue) ? performanceValue : null;

            return new DewisClubPlayerLeagueDetails
            {
                OldDWZ = dwzOld,
                AverageOpponentDWZ = opponentDwz,
                NewDWZ = dwzNew,
                Performance = performance
            };
        }

        private static bool TournamentNameMatchesExpectedLeague(string tournamentName, League league)
        {
            var region = league.Region;
            var leagueName = league.Name;
            if (leagueName.Contains("Oberliga Baden"))
            {
                return tournamentName.Contains("Oberliga Baden 2025/26");
            }
            else if (leagueName.Contains("Verbandsliga Nord") || leagueName.Contains("Landesliga Nord") || leagueName.Contains("Bereichsliga Nord"))
            {
                return tournamentName.Contains("Verbandsrunde Baden Nord 2025/26");
            }
            else if (leagueName.Contains("Verbandsliga Süd") || leagueName.Contains("Landesliga Süd") || leagueName.Contains("Bereichsliga Süd"))
            {
                return tournamentName.Contains("Verbandsrunde Baden Süd 2025/26");
            }
            else if (region.Contains("Mannheim"))
            {
                return tournamentName.Contains("Verbandsrunde Mannheim 2025/26");
            }
            else if (region.Contains("Heidelberg"))
            {
                return tournamentName.Contains("Schachbezirk Heidelberg Verbandsspiele 2025/26");
            }
            else if (region.Contains("Odenwald"))
            {
                return tournamentName.Contains("Schachbezirk Heidelberg Verbandsspiele 2025/26");
            }
            else if (region.Contains("Karlsruhe"))
            {
                return tournamentName.Contains("Verbandsrunde 25/26 Bezirk Karlsruhe");
            }
            else if (region.Contains("Pforzheim"))
            {
                return tournamentName.Contains("Schachbezirk Pforzheim VB 2025/26");
            }
            else if (region.Contains("Mittelbaden"))
            {
                return tournamentName.Contains("Verbandsrunde Mittelbaden 2025/26");
            }
            else if (region.Contains("Ortenau"))
            {
                if (leagueName.Contains("Bezirksklasse"))
                {
                    return tournamentName.Contains("Bezirksmannschaftsmeisterschaft Bezirksklasse Saison 2025/2026");
                }
                else if (leagueName.Contains("Kreisklasse A"))
                {
                    return tournamentName.Contains("Bezirksmannschaftsmeisterschaft Kreisklasse A Saison 2025/2026");
                }
                else if (leagueName.Contains("Kreisklasse B"))
                {
                    return tournamentName.Contains("Bezirksmannschaftsmeisterschaft Kreisklasse B Saison 2025/2026");
                }
                else if (leagueName.Contains("Jugendliga"))
                {
                    return tournamentName.Contains("Bezirksmannschaftsmeisterschaft Jugend- und Kinderliga Saison 2025/2026");
                }
            }
            else if (region.Contains("Freiburg"))
            {
                if (leagueName.Contains("Bezirksklasse"))
                {
                    return tournamentName.Contains("Verbandsrunde 2025-2026 Bezirksklasse Freiburg");
                }
                else if (leagueName.Contains("Kreisklasse A"))
                {
                    return tournamentName.Contains("Verbandsrunde 2025-2026 Kreisklasse A Freiburg");
                }
                else if (leagueName.Contains("Kreisklasse B"))
                {
                    return tournamentName.Contains("Verbandsrunde 2025-2026 Kreisklasse B Freiburg");
                }
                else if (leagueName.Contains("Kreisklasse C"))
                {
                    return tournamentName.Contains("Verbandsrunde 2025-2026 Kreisklasse C Freiburg");
                }
                else if (leagueName.Contains("Kreisklasse D"))
                {
                    return tournamentName.Contains("Verbandsrunde 2025-2026 Kreisklasse D Freiburg");
                }
            }
            else if (region.Contains("Hochrhein"))
            {
                return tournamentName.Contains("Bezirk Hochrhein") && tournamentName.Contains("2025") && tournamentName.Contains("2026");
            }
            else if (region.Contains("Schwarzwald"))
            {
                return tournamentName.Contains("Bezirksklasse Schwarzwald 2025/2026");
            }
            else if (region.Contains("Bodensee"))
            {
                return tournamentName.Contains("Bezirksklasse Bodensee Saison 2025/26");
            }
            return false;
        }
    }
}
