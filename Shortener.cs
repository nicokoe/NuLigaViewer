using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace NuLigaViewer;

public class Shortener
{
    public static readonly Shortener Instance;

    static Shortener()
    {
        Instance = new Shortener();
    }

    private Shortener() { }

    [Flags]
    public enum Flags
    {
        None, ClubNamePrefix = 1, ClubNameBad = 2,
        AbbrevWithPoint = 32,
    }

    public string? ShortenClubName(string? name)
    {
        if (name != null)
        {
            if ((NameFlags & Flags.ClubNamePrefix) != 0)
            {
                var m = rePrefix2.Match(name); // SC Wiesloch / SF Baiertal 1  => Wiesloch/Baiertal 1
                if (m.Success)
                    name = m.Groups[1].Value + "/" + m.Groups[2].Value + m.Groups[3].Value;
                else
                {
                    m = rePrefix.Match(name);  // OSG Baden-Baden 1 => Baden-Baden 1  u.ä.
                    if (m.Success)
                        name = m.Groups[1].Value;
                }
            }

            if ((NameFlags & Flags.ClubNameBad) != 0)
            {
                var m = reBad.Match(name); // Bad Mergentheim => Mergentheim  u.ä.
                if (m.Success)
                    name = m.Groups[1].Value + m.Groups[2].Value;
            }

            if (ClubNameNumChars > 0)
            {
                // Wiesloch/Baiertal 4, Niefern-Öschelbronn 2 o.ä.
                // - Die führenden SC/SF.. sind schon oben weg gemacht worden.
                var m = reTrunc1.Match(name);
                if (m.Success)
                {
                    var k = ClubNameNumChars / 2;
                    var l = ClubNameNumChars - k;
                    name = m.Groups[1].Value.Truncate(k, Ellipsis) + m.Groups[2].Value +
                        m.Groups[3].Value.Truncate(l, Ellipsis) +
                        m.Groups[4].Value;
                }
                else
                {
                    m = reTrunc2.Match(name);  // Ittersbach 2 u.ä.
                    if (m.Success)
                        name = m.Groups[1].Value.Truncate(ClubNameNumChars, Ellipsis)
                            + m.Groups[2].Value;
                    else
                    {
                        m = reTrunc3.Match(name);  // Heilbronner SV u.ä.
                        if (m.Success)
                            name = m.Groups[1].Value.Truncate(ClubNameNumChars, Ellipsis) + m.Groups[3].Value;
                        else
                        {
                            m = reTrunc4.Match(name);  // Slavija Karlsruhe u.ä.
                            if (m.Success)
                                name = m.Groups[1].Value.Truncate(ClubNameNumChars, Ellipsis) + m.Groups[2].Value;
                        }
                    }
                }
            }
        }

        return name;
    }

    public string? ShortenPlayerName(string? name)
    {
        if (name != null && (PlayerFirstNameNumChars >= 0 || PlayerSurNameNumChars >= 0))
        {
            var m = rePlayer.Match(name);
            if (m.Success)
            {
                var firstName = m.Groups[2].Value;
                var surName = m.Groups[1].Value;

                if (PlayerFirstNameNumChars > 0)
                    firstName = ", " + firstName.Truncate(PlayerFirstNameNumChars, Ellipsis);
                else if (PlayerFirstNameNumChars == 0)
                    firstName = "";
                else if (PlayerFirstNameNumChars == -1)
                    firstName = ", " + firstName;

                if (PlayerSurNameNumChars > 0)
                {
                    // Wenn ich schon den Nachnamen abkürzen muß, werf ich auch Prof. Dr. weg. 
                    m = reProfDr.Match(surName);
                    if (m.Success)
                    {
                        surName = m.Groups["name"].Value;
                        if (surName.Length + m.Groups["dr"].Value.Length <= PlayerSurNameNumChars)
                            surName = m.Groups["dr"].Value + surName;

                    }
                    surName = surName.Truncate(PlayerSurNameNumChars, Ellipsis);
                }

                name = surName + firstName;
            }
        }
        return name;
    }

    public void SetShortenClubName(string c)
    {
        ShortenClubNameChar = c;
        if ("23456".Contains(c))
        {
            NameFlags |= Flags.ClubNamePrefix | Flags.ClubNameBad | Flags.AbbrevWithPoint;
            var dd = "3:10 4:8 5:6 6:4".ToDictionary(" ", ":");
            if (dd.TryGetValue(c, out string? n))
                ClubNameNumChars = Convert.ToInt16(n);
            else
                ClubNameNumChars = 0;
        }
        else
        {
            NameFlags &= ~(Flags.ClubNamePrefix | Flags.ClubNameBad);
            ClubNameNumChars = 0;
        }
    }

    public string ShortenClubNameChar { get; private set; } = "1";

    public void SetShortenPlayerName(string c)
    {
        ShortenPlayerNameChar = c;
        PlayerFirstNameNumChars = -1;
        PlayerSurNameNumChars = -1;
        if ("234567".Contains(c))
        {
            NameFlags |= Flags.AbbrevWithPoint;
            var dd = "2:6 3:4 4:2 5:0 6:0 7:0".ToDictionary(" ", ":");
            if (dd.TryGetValue(c, out string? n))
                PlayerFirstNameNumChars = Convert.ToInt16(n);
            var ds = "6:10 7:8".ToDictionary(" ", ":");
            if (ds.TryGetValue(c, out string? m))
                PlayerSurNameNumChars = Convert.ToInt16(m);
        }
    }

    public string ShortenPlayerNameChar { get; private set; } = "1";

    public Flags NameFlags { get; set; } = Flags.ClubNamePrefix | Flags.ClubNameBad | Flags.AbbrevWithPoint;
    private int ClubNameNumChars { get; set; } = 7;
    private int PlayerFirstNameNumChars { get; set; } = 5;
    private int PlayerSurNameNumChars { get; set; } = -1;


    public bool IsAbbrevWithPoint => (NameFlags & Flags.AbbrevWithPoint) != 0;
    private string Ellipsis => IsAbbrevWithPoint ? "." : "";

    static readonly Regex rePrefix = new Regex(@"^(?:[A-Z]+\s+)(.*)");
    static readonly Regex rePrefix2 = new Regex(@"^(?:[A-Z]+\s+)(\S+)\s*/\s*(?:[A-Z]+\s+)(\S+)\s*(\s\d*)$");
    static readonly Regex reBad = new Regex(@"^(\s*)(?:Bad\s+)(.*)");
    static readonly Regex reTrunc1 = new Regex(@"^(\S+)\s*([-/])\s*(\S+)\s*(\s\d*)$");
    static readonly Regex reTrunc2 = new Regex(@"^(\S+)\s*(\s\d*)$");
    static readonly Regex reTrunc3 = new Regex(@"^(\S+)\s*(\s[A-Z]+)(\s\d*)$");
    static readonly Regex reTrunc4 = new Regex(@"^(\S+\s\D+)(\s\d*)$");
    static readonly Regex rePlayer = new Regex(@"^\s*(.*),\s*(.*)\s*$");
    static readonly Regex reProfDr = new Regex(@"^(?:Prof\.\s*)?(?<dr>Dr\.\s*)?(?<name>.*)");

}
