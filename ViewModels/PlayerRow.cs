using NuLigaViewer.Data;
using System.Data;
using System.Windows.Input;

namespace NuLigaViewer.ViewModels
{
    public class PlayerRow
    {
        public PlayerRow()
        {
            _backButtonCommand = new RelayCommand(AppShell.GoBackInStack, () => true);
        }

        private readonly RelayCommand _backButtonCommand;
        public ICommand BackButtonCommand => _backButtonCommand;

        public int Brett { get; set; }
        public string Spieler 
        { 
            get { return spieler; }
            set { spieler = value; UISpieler = Shortener.Instance.ShortenPlayerName(value); }
        }
        private string spieler = string.Empty;
        public string UISpieler { get; private set; } = string.Empty;
        public int? DWZ { get; set; }
        public List<string> Rounds { get; set; } = new();
        public List<PlayerGameDayInfo?> PlayerGameDayInfos { get; set; } = new();
        public IEnumerable<PlayerGameDayInfo> RegularGameDayInfos => PlayerGameDayInfos.Where(x => x != null && x.Pairing.BoardPoints.IsRegularResult() && !x.SecExists).Select(x => x!);
        public string Total { get; set; } = string.Empty;

        public int AverageOpponentDWZ => RegularGameDayInfos.Count() > 0 ? (int)Math.Round(RegularGameDayInfos.Average(x => x!.OpponentDWZ ?? 0)) : 0;
        public int GamesPlayed => RegularGameDayInfos.Count();
        public double PointsSum => RegularGameDayInfos.Sum(x => x.Points);
        public double SumOfExpectedPoints => DWZ == null ? 0 : RegularGameDayInfos.Sum(x => x.OpponentDWZ.HasValue ? PofD(DWZ.Value - x.OpponentDWZ.Value) : 0);
        public int Entwicklungskoeffizient => ComputeEntwicklungskoeffizient();
        public int? Performance => (DWZ == null || GamesPlayed == 0) ? null : DWZ + (int)Math.Round((PointsSum - SumOfExpectedPoints) * 800.0 / (double)GamesPlayed);
        public int? EstimatedNewDWZ => (DWZ == null || GamesPlayed == 0) ? null : DWZ + (int)Math.Round((PointsSum - SumOfExpectedPoints) / (Entwicklungskoeffizient + GamesPlayed) * 800);
        public string? RatingDifference => (EstimatedNewDWZ - DWZ) > 0 ? "+" + (EstimatedNewDWZ - DWZ) : (EstimatedNewDWZ - DWZ).ToString();
        public string EstimatedNewDwzString => $"{EstimatedNewDWZ} ({RatingDifference})";

        private int ComputeEntwicklungskoeffizient()
        {
            var relDwz = (DWZ ?? 0) / 1000.0;
            var grundwert = Math.Pow(relDwz, 4) + 15;
            var bremszuschlag = 0;
            if (DWZ < 1300 && PointsSum <= SumOfExpectedPoints)
            {
                bremszuschlag = (int)Math.Round((Math.Pow(Math.E, (1300 - DWZ ?? 0) / 150.0) - 1));
            }
            var entwicklungskoeffizient = grundwert + bremszuschlag;

            if (bremszuschlag == 0 && entwicklungskoeffizient > 30)
            {
                return 30;
            }
            else if (entwicklungskoeffizient > 150)
            {
                return 150;
            }
            return (int)Math.Round(entwicklungskoeffizient);
        }

        private double PofD(int diff)
        {
            if (diff < 0)
            {
                return 1 - PofD(-diff);
            }

            if (diff < 4)
            {
                return 0.5;
            }
            else if (diff < 11)
            {
                return 0.51;
            }
            else if (diff < 18)
            {
                return 0.52;
            }
            else if (diff < 25)
            {
                return 0.53;
            }
            else if (diff < 32)
            {
                return 0.54;
            }
            else if (diff < 40)
            {
                return 0.55;
            }
            else if (diff < 47)
            {
                return 0.56;
            }
            else if (diff < 54)
            {
                return 0.57;
            }
            else if (diff < 61)
            {
                return 0.58;
            }
            else if (diff < 69)
            {
                return 0.59;
            }
            else if (diff < 76)
            {
                return 0.60;
            }
            else if (diff < 83)
            {
                return 0.61;
            }
            else if (diff < 91)
            {
                return 0.62;
            }
            else if (diff < 98)
            {
                return 0.63;
            }
            else if (diff < 106)
            {
                return 0.64;
            }
            else if (diff < 113)
            {
                return 0.65;
            }
            else if (diff < 121)
            {
                return 0.66;
            }
            else if (diff < 129)
            {
                return 0.67;
            }
            else if (diff < 137)
            {
                return 0.68;
            }
            else if (diff < 145)
            {
                return 0.69;
            }
            else if (diff < 153)
            {
                return 0.70;
            }
            else if (diff < 161)
            {
                return 0.71;
            }
            else if (diff < 170)
            {
                return 0.72;
            }
            else if (diff < 178)
            {
                return 0.73;
            }
            else if (diff < 187)
            {
                return 0.74;
            }
            else if (diff < 196)
            {
                return 0.75;
            }
            else if (diff < 205)
            {
                return 0.76;
            }
            else if (diff < 214)
            {
                return 0.77;
            }
            else if (diff < 224)
            {
                return 0.78;
            }
            else if (diff < 234)
            {
                return 0.79;
            }
            else if (diff < 244)
            {
                return 0.80;
            }
            else if (diff < 254)
            {
                return 0.81;
            }
            else if (diff < 265)
            {
                return 0.82;
            }
            else if (diff < 276)
            {
                return 0.83;
            }
            else if (diff < 288)
            {
                return 0.84;
            }
            else if (diff < 300)
            {
                return 0.85;
            }
            else if (diff < 312)
            {
                return 0.86;
            }
            else if (diff < 326)
            {
                return 0.87;
            }
            else if (diff < 340)
            {
                return 0.88;
            }
            else if (diff < 355)
            {
                return 0.89;
            }
            else if (diff < 371)
            {
                return 0.9;
            }
            else if (diff < 389)
            {
                return 0.91;
            }
            else if (diff < 408)
            {
                return 0.92;
            }
            else if (diff < 429)
            {
                return 0.93;
            }
            else if (diff < 453)
            {
                return 0.94;
            }
            else if (diff < 480)
            {
                return 0.95;
            }
            else if (diff < 513)
            {
                return 0.96;
            }
            else if (diff < 555)
            {
                return 0.97;
            }
            else if (diff < 614)
            {
                return 0.98;
            }
            else
            {
                return 0.99;
            }
        }
    }
}