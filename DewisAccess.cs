using NuLigaViewer.Data;

namespace NuLigaViewer
{
    public static class DewisAccess
    {
        public static async Task<Dictionary<string, DewisClubPlayer>?> GetClubPlayers(string zpsNumber)
        {
            var players = new Dictionary<string, DewisClubPlayer>();
            var url = $"https://www.schachbund.de/php/dewis/verein.php?zps={zpsNumber}&format=csv";

            using (HttpClient client = new())
            {
                HttpResponseMessage response = await client.GetAsync(url);
                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    var contentLines = content.Split(Environment.NewLine);

                    foreach (var line in contentLines)
                    {
                        if (line.StartsWith("id|nachname|vorname|titel|verein|mglnr|status|dwz|dwzindex|turniercode|turnierende|fideid|fideelo|fidetitel"))
                        {
                            continue;
                        }
                        var splittedEntries = line.Split('|');
                        if (splittedEntries.Length != 14)
                        {
                            continue;
                        }

                        var nn = splittedEntries[1];
                        var vn = splittedEntries[2];

                        players[$"{nn}, {vn}"] = new DewisClubPlayer
                        {
                            Id = int.TryParse(splittedEntries[0], out var playerId) ? playerId : null,
                            Nachname = nn,
                            Vorname = vn,
                            Titel = splittedEntries[3],
                            DWZ = int.TryParse(splittedEntries[7], out var dwz) ? dwz : null,
                        };
                    }
                }
            }

            return players;
        }
    }
}
