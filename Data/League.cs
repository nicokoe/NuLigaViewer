namespace NuLigaViewer.Data
{
    public class League
    {
        public required string Name { get; set; }
        public required string Year { get; set; }
        public required string Region { get; set; }
        public required Category Category { get; set; }
        public required string Url { get; set; }
    }
}