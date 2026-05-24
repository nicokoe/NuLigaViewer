namespace NuLigaViewer.Data
{
    public class Player
    {
        public int Brett { get; set; }
        public string? Name { get; set; }
        public string? PlayerUrl { get; set; }
        public int? MemberNumber { get; set; }
        public int DWZ { get; set; }

        public int Games { get; set; }
        public string? BoardPoints { get; set; }
        public string? TeamName { get; set; }
        public double Points => BoardPoints?.ToPoints() ?? 0.0;

        public PlayerGameDayInfo?[]? PlayerInfoPerGameDay { get; set; }

        public override string ToString()
        {
            return $"{Brett}. {Name} (DWZ: {DWZ}) - Games: {Games}";
        }
    }

    public class PlayerGameDayInfo
    {
        public required Pairing Pairing { get; set; }
        public double Points => Pairing.BoardPoints.ToDouble(PlayerIsInHomeTeam);
        public bool PlayerIsInHomeTeam { get; set; }

        public string? ResultForPlayer => Pairing?.BoardPoints.AsString(PlayerIsInHomeTeam);
        public string? Opponent => PlayerIsInHomeTeam ? Pairing?.GastSpieler : Pairing?.HeimSpieler;
        public int? OpponentDWZ => PlayerIsInHomeTeam ? Pairing?.GastSpielerDWZ : Pairing?.HeimSpielerDWZ;
        public string? OpponentDWZString => PlayerIsInHomeTeam ? Pairing?.VisibleGuestDWZ : Pairing?.VisibleHomeDWZ;
        public string? OpponentTeam => PlayerIsInHomeTeam ? Pairing?.RelatedTeamPairing?.GastMannschaft : Pairing?.RelatedTeamPairing?.HeimMannschaft;

        public Pairing? SecondPairing { get; set; }
        public bool SecExists => SecondPairing != null;
        public string? SecResultForPlayer => SecondPairing?.BoardPoints.AsString(PlayerIsInHomeTeam);
        public string? SecOpponent => PlayerIsInHomeTeam ? SecondPairing?.GastSpieler : SecondPairing?.HeimSpieler;
        public int? SecOpponentDWZ => PlayerIsInHomeTeam ? SecondPairing?.GastSpielerDWZ : SecondPairing?.HeimSpielerDWZ;
        public string? SecOpponentDWZString => PlayerIsInHomeTeam ? SecondPairing?.VisibleGuestDWZ : SecondPairing?.VisibleHomeDWZ;
        public string? SecOpponentTeam => PlayerIsInHomeTeam ? SecondPairing?.RelatedTeamPairing?.GastMannschaft : SecondPairing?.RelatedTeamPairing?.HeimMannschaft;
    }
}