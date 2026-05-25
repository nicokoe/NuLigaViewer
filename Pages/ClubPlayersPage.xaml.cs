using NuLigaViewer.ViewModels;

namespace NuLigaViewer.Pages
{
    public partial class ClubPlayersPage : ContentPage
    {
        public ClubPlayersPage()
        {
            InitializeComponent();

            BindingContext = NavigationState.SelectedTeamOverview;
        }

        async void OnBackButtonClicked(object? sender, EventArgs e)
        {
            await Shell.Current.GoToAsync($"//league/table");
        }

        async void OnPlayerSelectionChanged(object? sender, SelectionChangedEventArgs e)
        {
            var clubPlayer = e.CurrentSelection.FirstOrDefault() as ClubPlayerViewModel;
            if (clubPlayer is null)
            {
                return;
            }

            if (sender is CollectionView cv)
            {
                cv.SelectedItem = null;
            }

            var playerName = Uri.EscapeDataString(clubPlayer.Name ?? string.Empty);
            var url = Uri.EscapeDataString(clubPlayer.PlayerUrl ?? string.Empty);
            var pkz = Uri.EscapeDataString(clubPlayer.DewisPkz?.ToString() ?? string.Empty);
            await Shell.Current.GoToAsync($"playerdetails?playerName={playerName}&url={url}&pkzNumber={pkz}");
        }
    }
}