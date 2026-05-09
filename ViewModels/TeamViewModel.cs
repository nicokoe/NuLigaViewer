using NuLigaViewer.Data;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data;

namespace NuLigaViewer.ViewModels
{
    public class TeamViewModel : INotifyPropertyChanged
    {
        private readonly Team _team;

        public TeamViewModel(Team team)
        {
            _team = team ?? throw new ArgumentNullException(nameof(team));
            Rank = _team.Rang;
            BuildPlayerRows();
        }

        public string? ClubLineUpsUrl => _team.ClubLineUpsUrl;
        public Qualification Qualification => _team.Qualification;
        public int Rank { get; set; }
        public string Name => Shortener.Instance.ShortenClubName(_team.Name);
        public int Games => _team.Spiele;
        public int Points => _team.Punkte;
        public double BoardPointsSum => _team.BP;
        public double AverageDwz => _team.DWZ;
        public double BerlinTieBreak => _team.BW;
        public bool AllReportsLoaded => _team.AllReportsLoaded;

        public ObservableCollection<PlayerRow> PlayerRows { get; } = new();
        public IReadOnlyList<string> RoundHeaders { get; private set; } = Array.Empty<string>();

        public Color RowColor => GetColorByQualification();

        private Color GetColorByQualification()
        {
            switch (Qualification)
            {
                case Qualification.Aufstieg:
                    return Colors.Green;
                case Qualification.Abstieg:
                    return Colors.Red;
                default:
                    return Application.Current?.RequestedTheme == AppTheme.Dark ? Colors.White : Colors.Black;
            }
        }

        public IEnumerable<Player> Players => _team.TeamPlayers ?? Enumerable.Empty<Player>();
        public IList<TeamPairing>? GameDays => _team.GameDays;

        public bool ContainsTeamPairing(TeamPairing teamPairing) =>
            _team.GameDays != null && _team.GameDays.Contains(teamPairing);

        private void BuildPlayerRows()
        {
            PlayerRows.Clear();

            // determine number of rounds from team.GameDays or first player's PairingsPerGameDay length
            var rounds = _team.GameDays?.Count
                         ?? _team.TeamPlayers?.FirstOrDefault()?.PlayerInfoPerGameDay?.Length
                         ?? 0;

            RoundHeaders = Enumerable.Range(1, Math.Max(0, rounds)).Select(i => i.ToString()).ToList();

            foreach (var p in _team.TeamPlayers ?? Enumerable.Empty<Player>())
            {
                var row = new PlayerRow
                {
                    Brett = p.Brett,
                    Spieler = p.Name ?? string.Empty,
                    DWZ = p.DWZ == 1000 ? null : p.DWZ,
                    Rounds = [],
                    PlayerGameDayInfos = p.PlayerInfoPerGameDay?.Where(x => x != null).ToList() ?? []
                };

                var totalPoints = 0.0;
                var totalGames = 0;

                for (var i = 0; i < rounds; i++)
                {
                    var pointsString = "-";
                    if (p.PlayerInfoPerGameDay != null && i < p.PlayerInfoPerGameDay.Length)
                    {
                        var points = p.PlayerInfoPerGameDay[i]?.Points ?? -1;
                        pointsString = points == -1 ? "-" : (points == 1000 ? "+" : points.ToString());
                        if (p.PlayerInfoPerGameDay[i]?.SecondPairing != null)
                        {
                            var secondPoints = p.PlayerInfoPerGameDay[i]!.SecondPairing!.BoardPoints.ToDouble(p.PlayerInfoPerGameDay[i]!.PlayerIsInHomeTeam);
                            if (secondPoints != -1)
                            {
                                pointsString += $" / {(secondPoints == 1000 ? "+" : secondPoints.ToString())}";
                                if (secondPoints >= 0)
                                {
                                    totalPoints += (secondPoints == 1000 ? 1 : secondPoints);
                                    totalGames++;
                                }
                            }
                        }
                        if (points >= 0)
                        {
                            totalPoints += (points == 1000 ? 1 : points);
                            totalGames++;
                        }
                    }

                    row.Rounds.Add(pointsString);
                }

                row.Total = $"{totalPoints}/{totalGames}";

                PlayerRows.Add(row);
            }

            OnPropertyChanged(nameof(RoundHeaders));
        }

        public void Refresh()
        {
            OnPropertyChanged(nameof(Rank));
            OnPropertyChanged(nameof(Name));
            OnPropertyChanged(nameof(Games));
            OnPropertyChanged(nameof(Points));
            OnPropertyChanged(nameof(BoardPointsSum));
            OnPropertyChanged(nameof(AverageDwz));
            OnPropertyChanged(nameof(BerlinTieBreak));

            BuildPlayerRows();
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged(string propertyName) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}