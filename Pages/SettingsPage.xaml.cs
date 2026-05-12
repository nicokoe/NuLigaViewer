using NuLigaViewer.ViewModels;

namespace NuLigaViewer.Pages
{
    public partial class SettingsPage : ContentPage
    {
        public SettingsPage()
        {
            InitializeComponent();
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();

            if (Application.Current != null)
            {
                try
                {
                    string font = (string)Application.Current.Resources["AppFontFamily"];
                    var pickerItem = pickerFontMap.FirstOrDefault(kv => kv.Value == font);
                    if (pickerItem.Key != null)
                        DDLFontWidth.SelectedItem = pickerItem.Key;

                    int scn = Convert.ToInt16("" + Shortener.Instance.ShortenClubNameChar) - 1;
                    DDLShortenClubName.SelectedIndex = scn;

                    int spn = Convert.ToInt16("" + Shortener.Instance.ShortenPlayerNameChar) - 1;
                    DDLShortenPlayerName.SelectedIndex = spn;
                }
                catch (Exception)
                {
                    // Sollte vllt geloggt werden. 
                }
            }
        }

        private void OnFontWidthChanged(object sender, EventArgs e)
        {
            var picker = (Picker)sender;
            var selected = (string)picker.SelectedItem;
            var font = pickerFontMap[selected];
            if (Application.Current != null)
                Application.Current.Resources["AppFontFamily"] = font;
            Preferences.Set("fontname", font);
        }

        static readonly Dictionary<string, string> pickerFontMap =
             "Normal=OpenSansRegular,95 %=BarlowRegular,88 %=SemiCondensed,78 %=Condensed,65 %=ExtraCondensed"
                .ToDictionary(",", "=");



        private void OnShortenClubNameChanged(object sender, EventArgs e)
        {
            var picker = (Picker)sender;
            var selected = (string)picker.SelectedItem;
            var c = "" + selected[0];
            Shortener.Instance.SetShortenClubName(c);
            Preferences.Set("shortenClubName", c);
        }

        private void OnShortenPlayerNameChanged(object sender, EventArgs e)
        {
            var picker = (Picker)sender;
            var selected = (string)picker.SelectedItem;
            var c = "" + selected[0];
            Shortener.Instance.SetShortenPlayerName(c);
            Preferences.Set("shortenPlayerName", c);
        }
    }
}