namespace NuLigaViewer.Data
{
    public class BadenRegion : List<League>
    {
        public string Name { get; set; }

        public BadenRegion(string name, List<League> leagues) : base(leagues)
        {
            Name = name;
        }
    }

    public class League
    {
        public required string Name { get; set; }
        public required string Url { get; set; }
    }
}