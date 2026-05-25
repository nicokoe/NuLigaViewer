using NuLigaViewer.Data;
using NuLigaViewer.ViewModels;

namespace NuLigaViewer.Pages
{
    public partial class PlayerPage : ContentPage, IQueryAttributable
    {
        public PlayerPage()
        {
            InitializeComponent();
        }

        public async void ApplyQueryAttributes(IDictionary<string, object> query)
        {
            if (query.TryGetValue("playerName", out var nameObj) && nameObj is string playerName)
            {
                var player = NavigationState.SelectedLeagueViewModel.AllAvailablePlayer.FirstOrDefault(player => player.Name == playerName);
                if (query.TryGetValue("url", out var urlObj) && urlObj is string url && !string.IsNullOrEmpty(url))
                {
                    try
                    {
                        var playerDetails = NuLigaParser.ParseClubPlayerDetails(url, playerName, NavigationState.SelectedTeamOverview.Name);
                        if (playerDetails != null)
                        {
                            player = playerDetails;
                        }
                    }
                    catch (Exception e)
                    {
                        System.Diagnostics.Debug.WriteLine(e.ToString());
                    }
                }
                if (player != null && query.TryGetValue("pkzNumber", out var pkzNumberObj) && pkzNumberObj is string pkzNumber && !string.IsNullOrEmpty(pkzNumber))
                {
                    player.DewisPkz = int.Parse(pkzNumber);
                }
                var leaguePerformance = await DewisAccess.GetClubPlayerLeaguePerformance(player?.DewisPkz?.ToString(), NavigationState.SelectedLeagueViewModel.League);

                BindingContext = player != null ? new PlayerRow
                {
                    Brett = player.Brett,
                    Spieler = player.Name ?? string.Empty,
                    DWZ = player.DWZ == 1000 ? null : player.DWZ,
                    Rounds = [],
                    PlayerGameDayInfos = player.PlayerInfoPerGameDay?.Where(x => x != null).ToList() ?? [],
                    LeaguePerformance = leaguePerformance
                } : null;
            }
        }

        async void OnPlayerSelectionChanged(object? sender, SelectionChangedEventArgs e)
        {
            var playerInfo = e.CurrentSelection.FirstOrDefault() as PlayerGameDayInfo;
            if (playerInfo is null)
            {
                return;
            }

            if (sender is CollectionView cv)
            {
                cv.SelectedItem = null;
            }

            var playerName = Uri.EscapeDataString(playerInfo.Opponent ?? string.Empty);
            var route = $"playerdetails?playerName={playerName}";
            if (playerInfo.Pairing.OpponentUrl != null)
            {
                route += $"&url={Uri.EscapeDataString(playerInfo.Pairing.OpponentUrl)}";
            }
            await Shell.Current.GoToAsync(route);
        }
    }
}