using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;

namespace BuildTask;

/// <summary>
/// Scans the system for installed JDKs and returns the best match.
/// Priority: manual override > JAVA_HOME/JDK_HOME env vars > registry (Windows) > common folders.
/// Prefers JDK 21, then 17. Picks the highest suitable version if multiple found.
/// </summary>
public static class JavaDiscovery
{
    private static readonly int[] PreferredVersions = [21, 17];

    public record JdkInfo(string Path, int MajorVersion);

    /// <summary>
    /// Discover the best JDK on this system. Returns null if none found.
    /// </summary>
    public static JdkInfo? FindBestJdk(string? manualOverride = null)
    {
        var candidates = new List<JdkInfo>();

        // 1. Manual override
        if (!string.IsNullOrWhiteSpace(manualOverride))
        {
            var info = ProbeJdk(manualOverride);
            if (info != null) return info;
        }

        // 2. Environment variables
        foreach (var envVar in new[] { "JAVA_HOME", "JDK_HOME" })
        {
            string? val = Environment.GetEnvironmentVariable(envVar);
            if (!string.IsNullOrWhiteSpace(val))
            {
                var info = ProbeJdk(val);
                if (info != null) candidates.Add(info);
            }
        }

        // 3. Platform-specific scanning
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            candidates.AddRange(ScanWindowsRegistry());
            candidates.AddRange(ScanFolders(WindowsJdkFolders()));
        }
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            candidates.AddRange(ScanFolders(MacJdkFolders()));
        }
        else
        {
            candidates.AddRange(ScanFolders(LinuxJdkFolders()));
        }

        if (candidates.Count == 0) return null;

        // Pick the best: prefer 21, then 17, then highest version
        var suitable = candidates
            .Where(c => PreferredVersions.Contains(c.MajorVersion))
            .OrderByDescending(c => c.MajorVersion)
            .FirstOrDefault();

        return suitable ?? candidates.OrderByDescending(c => c.MajorVersion).First();
    }

    /// <summary>
    /// Probe a single directory to see if it's a valid JDK.
    /// Checks for javac and extracts the version.
    /// </summary>
    private static JdkInfo? ProbeJdk(string jdkPath)
    {
        if (string.IsNullOrWhiteSpace(jdkPath) || !Directory.Exists(jdkPath))
            return null;

        string javac = RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
            ? Path.Combine(jdkPath, "bin", "javac.exe")
            : Path.Combine(jdkPath, "bin", "javac");

        if (!File.Exists(javac))
            return null;

        // Try to get version from javac -version
        int? version = GetVersionFromJavac(javac);
        if (version != null)
            return new JdkInfo(jdkPath, version.Value);

        // Fallback: try to parse version from folder name
        version = ParseVersionFromPath(jdkPath);
        if (version != null)
            return new JdkInfo(jdkPath, version.Value);

        return null;
    }

    private static int? GetVersionFromJavac(string javacPath)
    {
        try
        {
            var psi = new ProcessStartInfo
            {
                FileName = javacPath,
                Arguments = "-version",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true,
            };

            using var proc = Process.Start(psi);
            if (proc == null) return null;

            // javac outputs to stderr on some versions, stdout on others
            string stdout = proc.StandardOutput.ReadToEnd();
            string stderr = proc.StandardError.ReadToEnd();
            proc.WaitForExit(5000);

            string output = !string.IsNullOrWhiteSpace(stdout) ? stdout : stderr;
            return ParseMajorVersion(output);
        }
        catch
        {
            return null;
        }
    }

    /// <summary>
    /// Parse major version from "javac 21.0.2" or "javac 1.8.0_362" style output.
    /// </summary>
    private static int? ParseMajorVersion(string output)
    {
        if (string.IsNullOrWhiteSpace(output)) return null;

        // Match patterns like "21.0.2", "17.0.1", "1.8.0"
        var match = Regex.Match(output, @"(\d+)(?:\.(\d+))?");
        if (!match.Success) return null;

        int major = int.Parse(match.Groups[1].Value);
        // JDK 8 and earlier used "1.x" versioning
        if (major == 1 && match.Groups[2].Success)
            major = int.Parse(match.Groups[2].Value);

        return major;
    }

    /// <summary>
    /// Try to extract version from folder name like "jdk-21", "jdk-21.0.2+13", "java-17-openjdk".
    /// </summary>
    private static int? ParseVersionFromPath(string path)
    {
        string folder = Path.GetFileName(path.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar));
        var match = Regex.Match(folder, @"(?:jdk|java)[- _]?(\d+)");
        if (match.Success && int.TryParse(match.Groups[1].Value, out int ver))
            return ver;
        return null;
    }

    // ── Windows ──────────────────────────────────────────────────────────────

    private static List<JdkInfo> ScanWindowsRegistry()
    {
        var results = new List<JdkInfo>();
        if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows)) return results;

        try
        {
            // AdoptOpenJDK / Eclipse Adoptium
            ScanRegistryKey(results, @"SOFTWARE\Eclipse Adoptium\JDK");
            ScanRegistryKey(results, @"SOFTWARE\AdoptOpenJDK\JDK");
            // Oracle / OpenJDK
            ScanRegistryKey(results, @"SOFTWARE\JavaSoft\JDK");
            ScanRegistryKey(results, @"SOFTWARE\JavaSoft\Java Development Kit");
        }
        catch
        {
            // Registry access can fail — not fatal
        }

        return results;
    }

    private static void ScanRegistryKey(List<JdkInfo> results, string keyPath)
    {
        try
        {
            using var baseKey = Microsoft.Win32.Registry.LocalMachine.OpenSubKey(keyPath);
            if (baseKey == null) return;

            foreach (string subKeyName in baseKey.GetSubKeyNames())
            {
                using var subKey = baseKey.OpenSubKey(subKeyName);
                string? javaHome = subKey?.GetValue("JavaHome")?.ToString()
                                ?? subKey?.GetValue("Path")?.ToString();

                if (!string.IsNullOrWhiteSpace(javaHome))
                {
                    var info = ProbeJdk(javaHome);
                    if (info != null) results.Add(info);
                }
            }
        }
        catch { /* ignore per-key failures */ }
    }

    private static IEnumerable<string> WindowsJdkFolders()
    {
        var roots = new[]
        {
            @"C:\Program Files\Java",
            @"C:\Program Files\Eclipse Adoptium",
            @"C:\Program Files\AdoptOpenJDK",
            @"C:\Program Files\Microsoft\jdk",
            @"C:\Program Files\Zulu",
            @"C:\Program Files\Amazon Corretto",
            @"C:\Program Files\BellSoft\LibericaJDK",
            @"C:\Program Files (x86)\Java",
        };

        foreach (var root in roots)
        {
            if (!Directory.Exists(root)) continue;
            foreach (var dir in Directory.GetDirectories(root))
                yield return dir;
        }
    }

    // ── macOS ────────────────────────────────────────────────────────────────

    private static IEnumerable<string> MacJdkFolders()
    {
        string jvmRoot = "/Library/Java/JavaVirtualMachines";
        if (!Directory.Exists(jvmRoot)) yield break;

        foreach (var dir in Directory.GetDirectories(jvmRoot))
        {
            string contentsHome = Path.Combine(dir, "Contents", "Home");
            if (Directory.Exists(contentsHome))
                yield return contentsHome;
            else
                yield return dir;
        }
    }

    // ── Linux ────────────────────────────────────────────────────────────────

    private static IEnumerable<string> LinuxJdkFolders()
    {
        var roots = new[]
        {
            "/usr/lib/jvm",
            "/usr/local/lib/jvm",
            "/usr/java",
        };

        foreach (var root in roots)
        {
            if (!Directory.Exists(root)) continue;
            foreach (var dir in Directory.GetDirectories(root))
                yield return dir;
        }
    }

    private static List<JdkInfo> ScanFolders(IEnumerable<string> folders)
    {
        var results = new List<JdkInfo>();
        foreach (var folder in folders)
        {
            var info = ProbeJdk(folder);
            if (info != null) results.Add(info);
        }
        return results;
    }
}
