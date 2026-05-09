using System.Linq;

namespace NuLigaViewer
{
    public partial class AppShell : Shell
    {
        public AppShell()
        {
            InitializeComponent();

            Routing.RegisterRoute("teampairing", typeof(Pages.TeamPairingPage));
            Routing.RegisterRoute("playerdetails", typeof(Pages.PlayerPage));
        }

        private const string _lastGameDayRoute = "//league/lastgameday";
        private const string _toptenRoute = "//league/topten";
        private const string _gamedaysRoute = "//team/gamedays";
        private const string _playersRoute = "//team/players";
        private const string _clubplayersRoute = "//team/clubplayers";

        protected async override void OnNavigating(ShellNavigatingEventArgs e)
        {
            base.OnNavigating(e);

            if (e.Source != ShellNavigationSource.ShellSectionChanged || e.Current is null || e.Target is null || e.Target.Location == _pageBeingPopped?.Location)
            {
                return;
            }

            var targetPath = e.Target.Location?.ToString() ?? string.Empty;
            var levels = targetPath.Trim('/').Count(c => c == '/') - 1;

            if (levels < 1)
            {
                return;
            }

            if (!(targetPath.StartsWith(_lastGameDayRoute)
                || targetPath.StartsWith(_toptenRoute)
                || targetPath.StartsWith(_gamedaysRoute)
                || targetPath.StartsWith(_playersRoute)
                || targetPath.StartsWith(_clubplayersRoute)))
            {
                return;
            }

            if (e.Cancel())
            {
                try
                {
                    if (targetPath.StartsWith(_lastGameDayRoute))
                    {
                        ClearStackFromTabSubpages(_lastGameDayRoute);
                        await Shell.Current.GoToAsync(_lastGameDayRoute);
                    }
                    else if (targetPath.StartsWith(_toptenRoute))
                    {
                        ClearStackFromTabSubpages(_toptenRoute);
                        await Shell.Current.GoToAsync(_toptenRoute);
                    }
                    else if (targetPath.StartsWith(_gamedaysRoute))
                    {
                        ClearStackFromTabSubpages(_gamedaysRoute);
                        await Shell.Current.GoToAsync(_gamedaysRoute);
                    }
                    else if (targetPath.StartsWith(_playersRoute))
                    {
                        ClearStackFromTabSubpages(_playersRoute);
                        await Shell.Current.GoToAsync(_playersRoute);
                    }
                    else if (targetPath.StartsWith(_clubplayersRoute))
                    {
                        ClearStackFromTabSubpages(_clubplayersRoute);
                        await Shell.Current.GoToAsync(_clubplayersRoute);
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine(ex);
                }
            }
        }

        private static void ClearStackFromTabSubpages(string baseRoute)
        {
            var reversedUris = Uri.ToArray();
            Uri.Clear();

            for (var i = reversedUris.Length - 1; i >= 0; i--)
            {
                var uriString = reversedUris[i].Location.ToString();
                if (!uriString.StartsWith(baseRoute) || uriString.Equals(baseRoute))
                {
                    Uri.Push(reversedUris[i]);
                }
            }
        }

        protected override void OnNavigated(ShellNavigatedEventArgs args)
        {
            base.OnNavigated(args);
            if (Uri == null || args.Previous == null)
            {
                return;
            }

            if (args.Current != null && args.Current.Location.ToString() == "//home")
            {
                Uri.Clear();
                return;
            }

            if (_pageBeingPopped == null || _pageBeingPopped.Location != args.Current?.Location)
            {
                Uri.Push(args.Previous);
                _pageBeingPopped = null;
            }
        }

        protected override bool OnBackButtonPressed()
        {
            if (Uri != null && Uri.Count > 0)
            {
                Dispatcher.Dispatch(async () =>
                {
                    await GoBackInStack();
                });
                return true;
            }
            return base.OnBackButtonPressed();
        }

        public async static Task GoBackInStack()
        {
            if (Uri != null && Uri.Count > 0)
            {
                var previousPage = Uri.Pop();
                _pageBeingPopped = previousPage;
                await Shell.Current.GoToAsync(previousPage);
            }
        }
        private static Stack<ShellNavigationState> Uri { get; } = new Stack<ShellNavigationState>();
        private static ShellNavigationState? _pageBeingPopped { get; set; }
    }
}