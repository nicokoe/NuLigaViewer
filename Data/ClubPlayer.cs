namespace NuLigaViewer.Data
{
    public class ClubPlayer
    {
        public int Rang { get; set; }
        public int? DWZ { get; set; }
        public string? Name { get; set; }
        public int? MemberNumber { get; set; }
        public int? Number { get; set; }
        public string? Status { get; set; }
        public string? Url { get; set; }
    }

    public class DewisClubPlayer
    {
        public int? Id { get; set; }
        public required string Nachname { get; set; }
        public required string Vorname { get; set; }
        public required string Titel { get; set; }
        public int? DWZ { get; set; }
    }
}