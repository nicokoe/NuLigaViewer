using NuLigaViewer.Data;

namespace NuLigaViewer
{
    public partial class MainPage : ContentPage
    {
        public MainPage()
        {
            InitializeComponent();

            var year = Preferences.Default.Get("year", "2026/27");
            var preferedCategory = Preferences.Default.Get("category", "Verbandsrunde");
            var category = (Category)Enum.Parse(typeof(Category), preferedCategory);
            var regions = NuLigaParser.ParseLeagues(year, category);

            NavigationState.SelectedRegionsViewModel.Year = year;
            NavigationState.SelectedRegionsViewModel.Category = category;
            NavigationState.SelectedRegionsViewModel.LoadLeaguesFromRegions(regions);
            BindingContext = NavigationState.SelectedRegionsViewModel;
        }

        public async void OnLeagueSelected(object sender, SelectionChangedEventArgs e)
        {
            var selectedLeague = (e.CurrentSelection?.FirstOrDefault() as League);
            if (selectedLeague == null)
            {
                return;
            }

            if (sender is CollectionView cv)
            {
                cv.SelectedItem = null;
            }

            _ = NavigationState.SelectedLeagueViewModel.LoadLeagueAsync(selectedLeague);
            await Shell.Current.GoToAsync($"//league/table");
        }
    }
}
