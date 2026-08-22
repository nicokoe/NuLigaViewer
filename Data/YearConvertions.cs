namespace NuLigaViewer.Data
{
    public static class YearConvertions
    {
        public static string ConvertYearToUrlFormat(string year)
        {
            switch (year)
            {
                case "2025/26":
                    return "25%2F26";
                case "2026/27":
                    return "26%2F27";
                default:
                    throw new ArgumentOutOfRangeException(nameof(year), $"Year {year} is not supported.");
            }
        }
    }
}