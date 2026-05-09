namespace NuLigaViewer.Data
{
    public class Team
    {
        public Qualification Qualification { get; set; }
        public int Rang { get; set; }
        public string Name { get; set; } = string.Empty;
        public double DWZ => (TeamPlayers != null && TeamPlayers.Count > 0) ? Math.Round(TeamPlayers.Sum(x => (double)(x.DWZ * x.Games)) / TeamPlayers.Sum(x => x.Games)) : 0;

        public required double[] BoardPointsPerRank { get; set; }
        public int Spiele { get; set; }
        public int Punkte { get; set; }
        public double BP { get; set; }

        public string? TeamUrl { get; set; }
        public string? ClubLineUpsUrl { get; set; }

        public List<Player>? TeamPlayers { get; set; }
        public Dictionary<string, DewisClubPlayer>? ClubPlayers { get; set; }
        public List<TeamPairing>? GameDays { get; set; }
        public bool AllReportsLoaded => GameDays != null && GameDays.Where(t => !string.IsNullOrEmpty(t.ReportUrl))
            .All(t => t.Report != null);
        public double BW => ComputeBerlinTieBreakSumOverAllGameDays();

        public double ComputeBerlinTieBreakSumOverAllGameDays()
        {
            var bwTotal = 0.0;
            foreach (var gameDay in GameDays ?? Enumerable.Empty<TeamPairing>())
            {
                if (gameDay.Report == null)
                {
                    continue;
                }

                bwTotal += gameDay.Report.ComputeBw(gameDay.HeimMannschaft == Name);
            }

            return bwTotal;
        }

        public void GameDayReportLoaded(TeamPairing gameDay)
        {
            if (gameDay.Report == null)
            {
                return;
            }

            var isHomeTeam = gameDay.HeimMannschaft == Name;

            foreach (var player in TeamPlayers ?? Enumerable.Empty<Player>())
            {
                var pairings = gameDay.Report.GetPairingForPlayer(player.Name, isHomeTeam);
                if (!pairings.Any())
                {
                    continue;
                }
                if (player.PlayerInfoPerGameDay != null)
                {
                    player.PlayerInfoPerGameDay[gameDay.RoundByCount - 1] = new PlayerGameDayInfo
                    {
                        Pairing = pairings.First(),
                        SecondPairing = pairings.Count() > 1 ? pairings.Last() : null,
                        PlayerIsInHomeTeam = isHomeTeam
                    };
                }
            }
        }

        public override string ToString()
        {
            var boardPointsPerRankStr = string.Join(", ", BoardPointsPerRank ?? Enumerable.Empty<double>());

            var playersStr = "Players:";
            foreach (var player in TeamPlayers ?? Enumerable.Empty<Player>())
            {
                playersStr += $"\n  {player}";
            }

            return $"{Rang}. {Name} - Games: {Spiele}, Points: {Punkte}, BoardPoints: {BP}, BoardPointsPerRank: {boardPointsPerRankStr}, {playersStr}";
        }
    }
}