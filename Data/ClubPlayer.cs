namespace NuLigaViewer.Data
{
    public class ClubPlayer : IPlayer
    {
        public int? DewisPkz { get; set; }
        public int Rang { get; set; }
        public string? Name { get; set; }
        public string? PlayerUrl { get; set; }
        public int? MemberNumber { get; set; }
        public int? DWZ { get; set; }
        public int? Number { get; set; }
        public string? Status { get; set; }
    }

    public class DewisClubPlayer
    {
        public int? Pkz { get; set; }
        public required string Nachname { get; set; }
        public required string Vorname { get; set; }
        public required string Titel { get; set; }
        public int? DWZ { get; set; }
    }
}