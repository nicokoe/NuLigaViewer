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
        PlayerFirstNameKill = 4, PlayerFirstName1Char = 8, PlayerFirstName3Chars = 16, 
        AbbrevWithPoint = 32
    }

    public string? ShortenClubName(string? name)
    {
        if (name != null)
        {
            if ((NameFlags & Flags.ClubNamePrefix) != 0)
            {
                var m = rePrefix.Match(name);  // OSG Baden-Baden => Baden-Baden   u.ä.
                if (m.Success)
                    name = m.Groups[1].Value;
            }

            if ((NameFlags & Flags.ClubNameBad) != 0)
            {
                var m = reBad.Match(name); // Bad Mergentheim => Mergentheim  u.ä.
                if (m.Success)
                    name = m.Groups[1].Value + m.Groups[2].Value;
            }

            if (ClubNameNumChars > 0)
            {
                var m = reTrunc1.Match(name); // Niefern-Öschelbronn u.ä.
                if (m.Success)
                {
                    var k = ClubNameNumChars / 2;
                    var l = ClubNameNumChars - k - 1;
                    name = m.Groups[1].Value.Truncate(k, Ellipsis) + "-" +
                        m.Groups[2].Value.Truncate(l, Ellipsis) +
                        m.Groups[3].Value;
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
        if (name != null)
        {
            var m = rePlayer.Match(name);
            if (m.Success)
            {
                if ((NameFlags & Flags.PlayerFirstNameKill) != 0)
                    name = m.Groups[1].Value;
                else if ((NameFlags & Flags.PlayerFirstName1Char) != 0)
                    name = m.Groups[1].Value + ", " + m.Groups[2].Value.Truncate(2, Ellipsis);
                else if ((NameFlags & Flags.PlayerFirstName3Chars) != 0)
                    name = m.Groups[1].Value + ", " + m.Groups[2].Value.Truncate(4, Ellipsis);
            }
        }
        return name;
    }

    public Flags NameFlags { get; set; } = Flags.ClubNamePrefix | Flags.ClubNameBad | Flags.PlayerFirstName1Char | Flags.AbbrevWithPoint;
    public int ClubNameNumChars { get; set; } = 7;
    public bool IsClubNameWithPoint => (NameFlags & Flags.AbbrevWithPoint) != 0;
    private string Ellipsis => IsClubNameWithPoint ? "." : "";

    static readonly Regex rePrefix = new Regex(@"^(?:[A-Z]+\s+)(.*)");
    static readonly Regex reBad = new Regex(@"^(\s*)(?:Bad\s+)(.*)");
    static readonly Regex reTrunc1 = new Regex(@"^(\S+)-(\S+)\s*(\s\d*)$");
    static readonly Regex reTrunc2 = new Regex(@"^(\S+)\s*(\s\d*)$");
    static readonly Regex reTrunc3 = new Regex(@"^(\S+)\s*(\s[A-Z]+)(\s\d*)$");
    static readonly Regex reTrunc4 = new Regex(@"^(\S+\s\D+)(\s\d*)$");
    static readonly Regex rePlayer = new Regex(@"^\s*(.*),\s*(.*)\s*$");

}
