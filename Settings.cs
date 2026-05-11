namespace NuLigaViewer
{
    public enum TextSize
    {
        Normal = 0,
        Halbschmal = 1,
        Schmal = 2,
        Extraschmal = 3
    }

    public class Settings
    {
        public TextSize TextSize { get; set; } = TextSize.Normal;
    }
}
