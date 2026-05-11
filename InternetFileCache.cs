using System.Text;
using System.Security.Cryptography;
using System.Text.RegularExpressions;

namespace NuLigaViewer;


public class InternetFileCache
{
    public static readonly InternetFileCache Instance;


    static InternetFileCache()
    {
        _urlRegexe = new();
        _urlRegexe.Add((new Regex(@"^https://www.schachbund.de/php/dewis/verein.php\?zps=(\w+)&format=csv$"), "dewis"));
        _urlRegexe.Add((new Regex(@"^https://bsv-schach.liga.nu/cgi-bin/WebObjects/nuLigaSCHACHDE.woa/wa/clubInfoDisplay\?club=(\d+)$"), "cID"));
        _preferredFilesRegex = new Regex("^(dewis|cID)");

        Instance = new InternetFileCache(TimeSpan.FromDays(1), TimeSpan.FromDays(7), TimeSpan.FromDays(200), 5000, 100);
    }

    /// <summary>
    /// Erstellt einen FileCache mit drei unterschiedlichen Altersgrenzen.
    /// 
    /// Die drei Stufen steuern:
    /// - wann ein Cache-Eintrag direkt verwendet wird,
    /// - wann er im Hintergrund aktualisiert wird,
    /// - und wann er endgültig gelöscht wird.
    /// </summary>
    /// <param name="maxAge1">
    /// Wenn die Datei jünger als maxAge1 ist, wird sie direkt von der Platte
    /// zurückgegeben, ohne dass ein Download erfolgt.
    /// </param>
    /// <param name="maxAge2">
    /// Wenn die Datei älter als maxAge1, aber jünger als maxAge2 ist,
    /// wird sie von der Platte zurückgegeben und gleichzeitig im Hintergrund
    /// neu von der URL geladen.
    /// </param>
    /// <param name="maxAge3">
    /// Dateien, die älter als maxAge3 sind, werden beim Cleanup gelöscht.
    /// Sie gelten als veraltet und werden beim nächsten Zugriff neu geladen.
    /// </param>
    /// <param name="maxSizeKB">
    /// Maximale Größe des gesamten Cache-Verzeichnisses in Kilobyte.
    /// Wird diese Größe überschritten, löscht das Cleanup auch jüngere Dateien
    /// (beginnend mit den ältesten), bis die Größe wieder unterhalb des Limits liegt.
    /// </param>
    /// <param name="cleanupThreshold"> Alle cleanupThreshold Zugriffe wird ein Cleanup des Caches durchgeführt. </param>
    private InternetFileCache(TimeSpan maxAge1, TimeSpan maxAge2, TimeSpan maxAge3, int maxSizeKB, int cleanupThreshold)
    {
        _maxAge1 = maxAge1;
        _maxAge2 = maxAge2;
        _maxAge3 = maxAge3;
        _maxSizeBytes = maxSizeKB * 1024L;
        _cleanupThreshold = Math.Max(1, cleanupThreshold);

        _cacheDir = Path.Combine(FileSystem.AppDataDirectory, "inetcache");
        Directory.CreateDirectory(_cacheDir);

        // Cleanup im Hintergrund
        _ = Task.Run(CleanupOldFiles);
    }

    public async Task<string> GetAsync(string url)
    {
        MaybeStartCleanup();
        string path = GetPathForUrl(url);
        SemaphoreSlim sem = GetLock(path);

        await sem.WaitAsync();
        try
        {
            if (File.Exists(path))
            {
                var age = DateTime.Now - File.GetLastWriteTime(path);

                // 1. Frisch genug => direkt zurückgeben
                if (age < _maxAge1)
                    return await File.ReadAllTextAsync(path);

                // 2. Mittelalt => zurückgeben + Hintergrund-Refresh
                if (age < _maxAge2)
                {
                    string content = await File.ReadAllTextAsync(path);

                    // Hintergrund-Refresh
                    _ = Task.Run(async () =>
                    {
                        try
                        {
                            using var client = new HttpClient();
                            string fresh = await client.GetStringAsync(url);
                            await File.WriteAllTextAsync(path, fresh);
                        }
                        catch
                        {
                            // Fehler ignorieren – Cache bleibt stabil
                        }
                    });

                    return content;
                }

                // 3. Älter als maxAge2 → neu laden
            }

            // Datei existiert nicht oder ist zu alt => neu laden
            using var http = new HttpClient();
            string downloaded = await http.GetStringAsync(url);
            await File.WriteAllTextAsync(path, downloaded);
            return downloaded;
        }
        finally
        {
            sem.Release();
        }
    }

    public string Get(string url)
    {
        // Ruft die async-Methode synchron auf
        return GetAsync(url).GetAwaiter().GetResult();
    }

    private SemaphoreSlim GetLock(string path)
    {
        lock (_locks)
        {
            if (!_locks.TryGetValue(path, out var sem))
            {
                sem = new SemaphoreSlim(1, 1);
                _locks[path] = sem;
            }
            return sem;
        }
    }

    private string GetPathForUrl(string url)
    {
        string? file = null;
        System.Diagnostics.Debug.WriteLine(url);
        foreach (var re in _urlRegexe)
        {
            var m = re.Item1.Match(url);
            if (m.Success)
            {
                file = re.Item2 + m.Groups[1].Value;
                break;
            }
        }

        if (file == null)
        {
            using var sha = SHA256.Create();
            byte[] hash = sha.ComputeHash(Encoding.UTF8.GetBytes(url));
            file = Convert.ToHexString(hash) + ".cache";
        }
        return Path.Combine(_cacheDir, file);
    }


    /// <summary> Erhöht den Zugriffszähler atomar und startet asynchron ein Cleanup,
    /// wenn die Schwelle erreicht ist. Verhindert parallele Cleanups. </summary>
    private void MaybeStartCleanup()
    {
        long count = Interlocked.Increment(ref _accessCounter);         // atomar erhöhen

        // noch nicht an der Schwelle
        if (count < _cleanupThreshold)
            return;

        // Versuche, das Cleanup zu starten; nur einer darf starten
        if (Interlocked.CompareExchange(ref _cleanupRunning, 1, 0) == 0)
        {
            // Zähler zurücksetzen (kleine Race-Conditions sind unkritisch)
            Interlocked.Exchange(ref _accessCounter, 0);

            // Cleanup im Hintergrund starten, nicht awaiten
            _ = Task.Run(async () =>
            {
                try
                {
                    await CleanupOldFiles();
                }
                finally
                {
                    // Flag zurücksetzen
                    Interlocked.Exchange(ref _cleanupRunning, 0);
                }
            });
        }
    }

    private async Task CleanupOldFiles()
    {
        var files = Directory.GetFiles(_cacheDir).Select(f => new FileInfo(f)).ToList();

        // 1. Alte Dateien löschen (älter als maxAge3)
        foreach (var file in files)
        {
            try
            {
                var age = DateTime.Now - file.LastWriteTime;
                if (age > _maxAge3)
                    file.Delete();
            }
            catch { }
        }

        // Liste nach Löschungen neu laden
        files = Directory.GetFiles(_cacheDir).Select(f => new FileInfo(f)).ToList();

        // 2. Größe prüfen
        long totalSize = files.Sum(f => f.Length);

        if (totalSize <= _maxSizeBytes)
            return;

        // 3. Zu groß => nach Preferredness und Alter sortieren und löschen

        var preferred = files.Where(fi => _preferredFilesRegex.IsMatch(fi.Name)).
            OrderBy(f => f.LastWriteTime).ToList();

        var nonpreferred = files.Except(preferred).
            OrderBy(f => f.LastWriteTime).ToList();

        nonpreferred.AddRange(preferred);
        var ordered = nonpreferred;

        foreach (var file in ordered)
        {
            try
            {
                totalSize -= file.Length;
                file.Delete();

                if (totalSize <= _maxSizeBytes)
                    break;
            }
            catch { }
        }

        await Task.CompletedTask;
    }


    private readonly TimeSpan _maxAge1, _maxAge2, _maxAge3;
    private readonly long _maxSizeBytes;
    private readonly string[] _preferredFileStarts;

    private readonly string _cacheDir;
    private readonly Dictionary<string, SemaphoreSlim> _locks = new();
    private readonly int _cleanupThreshold;
    private long _accessCounter = 0;
    private int _cleanupRunning = 0; // 0 = nicht laufend, 1 = läuft

    static readonly List<(Regex, string)> _urlRegexe;
    static readonly Regex _preferredFilesRegex;
}
