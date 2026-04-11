using System.Text.RegularExpressions;

namespace BuildTask;

/// <summary>
/// Generates a complete Fabric mod template directory from ModInfo attribute values.
/// Creates build.gradle, gradle.properties, settings.gradle, fabric.mod.json,
/// and copies the Gradle wrapper so the user never touches Gradle manually.
/// </summary>
public static class FabricTemplateGenerator
{
    // ── Version lookup tables ─────────────────────────────────────────────────
    // Maps Minecraft version → (Yarn mappings, Loader version, Fabric API version, Loom version, Gradle version)
    // Source: https://fabricmc.net/develop  — verify before publishing a mod.
    // For versions not listed, set YarnMappings/FabricLoaderVersion/FabricApiVersion/LoomVersion in [ModInfo].

    private static readonly Dictionary<string, (string Yarn, string Loader, string FabricApi, string Loom, string Gradle)> VersionTable = new()
    {
        // ── 26.x (year-based versioning, Minecraft Java 2026+) ───────────────
        // Verify exact values at https://fabricmc.net/develop before publishing.
        ["26.2"]   = ("26.2+build.1",      "0.17.4", "0.135.0+26.2",    "1.13.0", "8.14"),
        ["26.1.2"] = ("26.1.2+build.1",    "0.17.4", "0.134.0+26.1.2",  "1.13.0", "8.14"),
        ["26.1.1"] = ("26.1.1+build.1",    "0.17.3", "0.133.0+26.1.1",  "1.12.4", "8.14"),
        ["26.1"]   = ("26.1+build.3",      "0.17.2", "0.131.0+26.1",    "1.12.4", "8.14"),

        // ── 1.21.x ───────────────────────────────────────────────────────────
        // 1.21.11 and 1.21.10: verify at fabricmc.net/develop
        ["1.21.11"] = ("1.21.11+build.1",  "0.16.14","0.129.0+1.21.11", "1.11.1", "8.13"),
        ["1.21.10"] = ("1.21.10+build.1",  "0.16.14","0.127.0+1.21.10", "1.11.1", "8.13"),
        ["1.21.9"]  = ("1.21.9+build.1",   "0.16.14","0.125.0+1.21.9",  "1.11.1", "8.13"),
        ["1.21.8"]  = ("1.21.8+build.1",   "0.16.14","0.124.0+1.21.8",  "1.10.4", "8.13"),
        ["1.21.7"]  = ("1.21.7+build.1",   "0.16.14","0.123.0+1.21.7",  "1.10.4", "8.13"),
        ["1.21.6"]  = ("1.21.6+build.1",   "0.16.14","0.122.0+1.21.6",  "1.10.4", "8.13"),
        ["1.21.5"]  = ("1.21.5+build.1",   "0.16.14","0.120.0+1.21.5",  "1.10.4", "8.13"),
        ["1.21.4"]  = ("1.21.4+build.8",   "0.16.10","0.119.5+1.21.4",  "1.9.5",  "8.11"),
        ["1.21.3"]  = ("1.21.3+build.2",   "0.16.9", "0.115.0+1.21.3",  "1.8.12", "8.11"),
        ["1.21.2"]  = ("1.21.2+build.1",   "0.16.7", "0.112.0+1.21.2",  "1.8.11", "8.11"),
        ["1.21.1"]  = ("1.21.1+build.3",   "0.16.5", "0.116.9+1.21.1",  "1.7.4",  "8.10"),
        ["1.21"]    = ("1.21+build.2",      "0.16.2", "0.100.0+1.21",    "1.7.4",  "8.10"),

        // ── 1.20.x ───────────────────────────────────────────────────────────
        ["1.20.6"] = ("1.20.6+build.3",    "0.15.11","0.97.8+1.20.6",   "1.6.12", "8.10"),
        ["1.20.5"] = ("1.20.5+build.1",    "0.15.11","0.97.3+1.20.5",   "1.6.12", "8.10"),
        ["1.20.4"] = ("1.20.4+build.3",    "0.15.11","0.97.2+1.20.4",   "1.5.8",  "8.8"),
        ["1.20.3"] = ("1.20.3+build.1",    "0.15.11","0.90.7+1.20.3",   "1.5.8",  "8.8"),
        ["1.20.2"] = ("1.20.2+build.4",    "0.15.11","0.91.6+1.20.2",   "1.5.8",  "8.6"),
        ["1.20.1"] = ("1.20.1+build.10",   "0.15.11","0.92.2+1.20.1",   "1.3.10", "8.6"),
        ["1.20"]   = ("1.20+build.1",      "0.14.22","0.83.0+1.20",     "1.3.10", "8.6"),
    };

    // ── Public API ────────────────────────────────────────────────────────────

    /// <summary>Returns true if the version string has an exact entry in the catalog.</summary>
    public static bool IsKnownVersion(string mcVersion) => VersionTable.ContainsKey(mcVersion);

    /// <summary>
    /// Parse [ModInfo(...)] attribute from C# source files. Returns null if not found.
    /// </summary>
    public static ModInfo? ParseModInfo(IEnumerable<string> sourcePaths)
    {
        foreach (var path in sourcePaths)
        {
            if (!File.Exists(path)) continue;
            var source = File.ReadAllText(path);
            var info = ParseModInfoFromSource(source);
            if (info != null) return info;
        }
        return null;
    }

    /// <summary>
    /// Generate the entire FabricTemplate directory at <paramref name="templatePath"/>.
    /// If the directory already exists and has a build.gradle, only regenerates
    /// fabric.mod.json and gradle.properties (preserves user customizations to build.gradle).
    /// </summary>
    public static void Generate(ModInfo info, string templatePath, string? gradleWrapperSourceDir)
{
    string modJsonPath = Path.Combine(templatePath, "src", "main", "resources", "fabric.mod.json");

    // ── SMART SKIP ──
    // If the template already exists, check if the ModId and Entrypoint match.
    // This prevents re-generating the whole structure on every single tiny code change.
    if (File.Exists(modJsonPath))
    {
        string existingJson = File.ReadAllText(modJsonPath);
        string expectedEntrypoint = $"{info.PackageName}.{info.ClassName}";
        
        if (existingJson.Contains($"\"id\": \"{info.Id}\"") && 
            existingJson.Contains($"\"{expectedEntrypoint}\""))
        {
            // The template is already up-to-date for this mod configuration.
            return; 
        }
    }

    // ── GENERATION LOGIC ──
    Directory.CreateDirectory(templatePath);

    var versions = GetVersions(info.MinecraftVersion, info);
    
    // Always write these as they are small and drive the build
    WriteGradleProperties(templatePath, info, versions);
    WriteFabricModJson(templatePath, info);

    // Only write the heavy boilerplate if it's missing
    if (!File.Exists(Path.Combine(templatePath, "build.gradle")))
    {
        WriteBuildGradle(templatePath, info);
        WriteSettingsGradle(templatePath);
        WriteGitignore(templatePath);

        // Create the source directory structure
        var packageDir = info.PackageName.Replace('.', Path.DirectorySeparatorChar);
        Directory.CreateDirectory(Path.Combine(templatePath, "src", "main", "java", packageDir));
        Directory.CreateDirectory(Path.Combine(templatePath, "src", "main", "resources", "assets", info.Id));

        // Copy Gradle wrapper from SDK bundle
        if (gradleWrapperSourceDir != null && Directory.Exists(gradleWrapperSourceDir))
        {
            CopyGradleWrapper(gradleWrapperSourceDir, templatePath);
        }
        else
        {
            WriteGradleWrapperProperties(templatePath, versions.Gradle);
            WriteGradlew(templatePath);
            WriteGradlewBat(templatePath);
        }
    }
}

    // ── Parsing ───────────────────────────────────────────────────────────────

    private static ModInfo? ParseModInfoFromSource(string source)
    {
        // Match [ModInfo( ... )] attribute on a class
        var attrMatch = Regex.Match(source,
            @"\[ModInfo\s*\((.*?)\)\s*\]",
            RegexOptions.Singleline);

        if (!attrMatch.Success) return null;

        string body = attrMatch.Groups[1].Value;

        string Get(string name, string fallback = "")
        {
            var m = Regex.Match(body, $@"{name}\s*=\s*""([^""]*?)""");
            return m.Success ? m.Groups[1].Value : fallback;
        }

        string id      = Get("Id");
        string name     = Get("Name");
        string version  = Get("Version", "1.0.0");
        string author   = Get("Author");
        string desc     = Get("Description");
        string mcVer    = Get("MinecraftVersion", "1.21.1");
        string pkg      = Get("PackageName");

        // Manual version overrides
        string yarn    = Get("YarnMappings");
        string loader  = Get("FabricLoaderVersion");
        string fabricApi = Get("FabricApiVersion");
        string loom    = Get("LoomVersion");

        if (string.IsNullOrWhiteSpace(id)) return null;
        if (string.IsNullOrWhiteSpace(name)) name = id;

        // Derive package name if not specified
        if (string.IsNullOrWhiteSpace(pkg))
        {
            string authorSlug = string.IsNullOrWhiteSpace(author)
                ? "example"
                : Regex.Replace(author.ToLowerInvariant(), @"[^a-z0-9]", "");
            pkg = $"com.{authorSlug}.{id.ToLowerInvariant()}";
        }

        // Extract the actual class name that the [ModInfo] attribute is on
        // Look for "class ClassName" after the attribute
        string className = id; // fallback
        int attrEnd = attrMatch.Index + attrMatch.Length;
        string afterAttr = source[attrEnd..];
        var classMatch = Regex.Match(afterAttr, @"\bclass\s+(\w+)");
        if (classMatch.Success)
            className = classMatch.Groups[1].Value;

        return new ModInfo(id, name, version, author, desc, mcVer, pkg, className,
            yarn, loader, fabricApi, loom);
    }

    // ── Version resolution ────────────────────────────────────────────────────

    /// <summary>
    /// Resolve Gradle build versions for a Minecraft version string.
    /// Checks the catalog first; falls back to the nearest older release for
    /// unknown versions (snapshots, pre-releases, future versions).
    /// Manual overrides in ModInfo always take precedence.
    /// </summary>
    private static (string Yarn, string Loader, string FabricApi, string Loom, string Gradle) GetVersions(
        string mcVersion, ModInfo? overrides = null)
    {
        // Start from catalog entry (or nearest fallback)
        var resolved = ResolveFromCatalog(mcVersion);

        // Apply manual overrides from [ModInfo] — any non-empty field wins
        if (overrides != null)
        {
            if (!string.IsNullOrWhiteSpace(overrides.YarnMappings))
                resolved.Yarn = overrides.YarnMappings;
            if (!string.IsNullOrWhiteSpace(overrides.FabricLoaderVersion))
                resolved.Loader = overrides.FabricLoaderVersion;
            if (!string.IsNullOrWhiteSpace(overrides.FabricApiVersion))
                resolved.FabricApi = overrides.FabricApiVersion;
            if (!string.IsNullOrWhiteSpace(overrides.LoomVersion))
                resolved.Loom = overrides.LoomVersion;
        }

        return resolved;
    }

    private static (string Yarn, string Loader, string FabricApi, string Loom, string Gradle) ResolveFromCatalog(string mcVersion)
    {
        // Exact match
        if (VersionTable.TryGetValue(mcVersion, out var exact))
            return exact;

        // Snapshot format: 25w14a, 26w11a etc. — find nearest release by year/week
        if (IsSnapshot(mcVersion, out int snapYear, out int snapWeek))
            return FindNearestRelease(snapYear, snapWeek);

        // Unknown release string — find nearest by semver
        if (TryParseRelease(mcVersion, out int maj, out int min, out int patch))
            return FindNearestReleaseBySemver(maj, min, patch);

        // Last resort — latest in table
        return VersionTable["26.2"];
    }

    private static bool IsSnapshot(string v, out int year, out int week)
    {
        year = 0; week = 0;
        var m = Regex.Match(v, @"^(\d{2})w(\d{2})[a-z]$");
        if (!m.Success) return false;
        year = int.Parse(m.Groups[1].Value);
        week = int.Parse(m.Groups[2].Value);
        return true;
    }

    private static bool TryParseRelease(string v, out int maj, out int min, out int patch)
    {
        maj = 0; min = 0; patch = 0;
        var parts = v.Split('.');
        if (parts.Length < 2) return false;
        if (!int.TryParse(parts[0], out maj)) return false;
        if (!int.TryParse(parts[1], out min)) return false;
        if (parts.Length >= 3) int.TryParse(parts[2], out patch);
        return true;
    }

    // Approximate Minecraft release calendar: map (year, week) → nearest release version.
    // Snapshots between two entries resolve to the earlier entry (closest by week distance).
    private static readonly (int Year, int Week, string McVersion)[] SnapshotCalendar =
    [
        // 1.21.x era (2024-2025)
        (24, 33, "1.21.1"),  // 24w33a snapshots → 1.21.1
        (24, 40, "1.21.2"),  // 24w40a snapshots → 1.21.2
        (24, 44, "1.21.3"),  // 24w44a snapshots → 1.21.3
        (24, 46, "1.21.4"),  // 24w46a snapshots → 1.21.4
        (25,  2, "1.21.5"),  // 25w02a snapshots → 1.21.5
        (25, 14, "1.21.5"),  // 1.21.5 release week
        (25, 25, "1.21.6"),  // 25w25a snapshots → 1.21.6
        (25, 28, "1.21.7"),  // 25w28a snapshots → 1.21.7
        (25, 31, "1.21.8"),  // 25w31a snapshots → 1.21.8
        (25, 40, "1.21.9"),  // 25w40a snapshots → 1.21.9
        (25, 41, "1.21.10"), // 25w41a snapshots → 1.21.10
        (25, 48, "1.21.11"), // 25w48a snapshots → 1.21.11
        // 26.x era (2025-2026, new year-based versioning)
        (25, 51, "26.1"),    // 25w51a snapshots → 26.1
        (26,  4, "26.1"),    // 26w04a snapshots → 26.1 (released 26w before 26.1 full)
        (26, 13, "26.1.1"),  // 26w13a → 26.1.1
        (26, 15, "26.1.2"),  // 26w15a → 26.1.2
        (26, 18, "26.2"),    // 26w18a → 26.2 (upcoming)
    ];

    private static (string Yarn, string Loader, string FabricApi, string Loom, string Gradle) FindNearestRelease(int year, int week)
    {
        // Walk calendar entries to find the best match
        string bestVersion = "26.2";
        int bestDist = int.MaxValue;
        int targetCode = year * 100 + week;
        foreach (var (y, w, ver) in SnapshotCalendar)
        {
            int dist = Math.Abs(targetCode - (y * 100 + w));
            if (dist < bestDist) { bestDist = dist; bestVersion = ver; }
        }
        return VersionTable[bestVersion];
    }

    private static (string Yarn, string Loader, string FabricApi, string Loom, string Gradle) FindNearestReleaseBySemver(int maj, int min, int patch)
    {
        // Find the closest version by semver distance.
        // 26.x versions are scored as 260000+minor*100+patch to sort after 1.21.x.
        (string Yarn, string Loader, string FabricApi, string Loom, string Gradle) best = VersionTable["26.2"];
        int bestDist = int.MaxValue;
        foreach (var kvp in VersionTable)
        {
            if (!TryParseRelease(kvp.Key, out int m, out int n, out int p)) continue;
            // Remap 26.x to a large ordinal so it sorts after all 1.x.x versions
            int ord = m >= 20 ? m * 10000 + n * 100 + p   // 1.21.4 → 121_04, 1.20.1 → 120_01
                              : 260000 + n * 100 + p;       // 26.1.2 → 260_102
            int target = maj >= 20 ? maj * 10000 + min * 100 + patch : 260000 + min * 100 + patch;
            int dist = Math.Abs(ord - target);
            if (dist < bestDist) { bestDist = dist; best = kvp.Value; }
        }
        return best;
    }

    // ── File generators ───────────────────────────────────────────────────────

    private static void WriteGradleProperties(string templatePath, ModInfo info, (string Yarn, string Loader, string FabricApi, string Loom, string Gradle) v)
    {
        var content = $"""
            # Auto-generated by CSCraft from [ModInfo] attribute — safe to customize
            org.gradle.jvmargs=-Xmx1G
            org.gradle.parallel=true
            org.gradle.configuration-cache=false

            minecraft_version={info.MinecraftVersion}
            yarn_mappings={v.Yarn}
            loader_version={v.Loader}
            loom_version={v.Loom}

            mod_version={info.Version}
            maven_group={info.PackageName}
            archives_base_name={info.Id}

            fabric_api_version={v.FabricApi}
            """;
        File.WriteAllText(Path.Combine(templatePath, "gradle.properties"), content);
    }

    private static void WriteBuildGradle(string templatePath, ModInfo info)
    {
        // No C# interpolation needed — all values come from gradle.properties
        var content = """
            buildscript {
                repositories {
                    maven { url = 'https://maven.fabricmc.net/' }
                    mavenCentral()
                    gradlePluginPortal()
                }
                dependencies {
                    classpath "net.fabricmc:fabric-loom:${project.loom_version}"
                }
            }

            apply plugin: 'fabric-loom'
            apply plugin: 'maven-publish'

            version = project.mod_version
            group   = project.maven_group
            base { archivesName = project.archives_base_name }

            repositories {
                maven { url = 'https://maven.fabricmc.net/' }
                mavenCentral()
            }

            dependencies {
                minecraft "com.mojang:minecraft:${project.minecraft_version}"
                mappings  "net.fabricmc:yarn:${project.yarn_mappings}:v2"
                modImplementation "net.fabricmc:fabric-loader:${project.loader_version}"
                modImplementation "net.fabricmc.fabric-api:fabric-api:${project.fabric_api_version}"
            }

            processResources {
                inputs.property "version", project.version
                filesMatching("fabric.mod.json") { expand "version": inputs.properties.version }
            }

            tasks.withType(JavaCompile).configureEach { it.options.release = 21 }

            java {
                sourceCompatibility = JavaVersion.VERSION_21
                targetCompatibility = JavaVersion.VERSION_21
            }
            """;
        File.WriteAllText(Path.Combine(templatePath, "build.gradle"), content);
    }

    private static void WriteSettingsGradle(string templatePath)
    {
        var content =
            "pluginManagement {\n" +
            "    repositories {\n" +
            "        maven {\n" +
            "            name = 'Fabric'\n" +
            "            url = 'https://maven.fabricmc.net/'\n" +
            "        }\n" +
            "        mavenCentral()\n" +
            "        gradlePluginPortal()\n" +
            "    }\n" +
            "}\n";
        File.WriteAllText(Path.Combine(templatePath, "settings.gradle"), content);
    }

    private static void WriteFabricModJson(string templatePath, ModInfo info)
    {
        // Use the actual C# class name for the entry point
        string className = !string.IsNullOrWhiteSpace(info.ClassName) ? info.ClassName : info.Id;
        string entryClass = $"{info.PackageName}.{info.ClassName}";

        string authors = string.IsNullOrWhiteSpace(info.Author) ? "" : $"\"{info.Author}\"";

        // JSON needs literal { }, and we also need Gradle's ${version} placeholder.
        // Build the content manually to avoid interpolation issues.
        var content =
            "{\n" +
            "    \"schemaVersion\": 1,\n" +
            $"    \"id\": \"{info.Id}\",\n" +
            "    \"version\": \"${version}\",\n" +
            $"    \"name\": \"{info.Name}\",\n" +
            $"    \"description\": \"{info.Description}\",\n" +
            "    \"authors\": [\n" +
            $"        {authors}\n" +
            "    ],\n" +
            "    \"contact\": {},\n" +
            "    \"license\": \"MIT\",\n" +
            $"    \"icon\": \"assets/{info.Id}/icon.png\",\n" +
            "    \"environment\": \"*\",\n" +
            "    \"entrypoints\": {\n" +
            "        \"main\": [\n" +
            $"            \"{entryClass}\"\n" +
            "        ]\n" +
            "    },\n" +
            "    \"depends\": {\n" +
            "        \"fabricloader\": \">=0.18.6\",\n" +
            $"        \"minecraft\": \"~{info.MinecraftVersion}\",\n" +
            "        \"java\": \">=21\",\n" +
            "        \"fabric-api\": \"*\"\n" +
            "    }\n" +
            "}";

        var resourceDir = Path.Combine(templatePath, "src", "main", "resources");
        Directory.CreateDirectory(resourceDir);
        File.WriteAllText(Path.Combine(resourceDir, "fabric.mod.json"), content);
    }

    private static void WriteGitignore(string templatePath)
    {
        var content = """
            # Gradle
            .gradle/
            build/
            !gradle/wrapper/gradle-wrapper.jar

            # IDE
            .idea/
            *.iml
            .vscode/
            *.swp

            # OS
            .DS_Store
            Thumbs.db

            # Runtime
            run/
            """;
        File.WriteAllText(Path.Combine(templatePath, ".gitignore"), content);
    }

    // ── Gradle wrapper ────────────────────────────────────────────────────────

    private static void CopyGradleWrapper(string sourceDir, string templatePath)
    {
        // Copy gradle-wrapper.jar
        var wrapperDir = Path.Combine(templatePath, "gradle", "wrapper");
        Directory.CreateDirectory(wrapperDir);

        var srcJar = Path.Combine(sourceDir, "gradle", "wrapper", "gradle-wrapper.jar");
        if (File.Exists(srcJar))
            File.Copy(srcJar, Path.Combine(wrapperDir, "gradle-wrapper.jar"), overwrite: true);

        var srcProps = Path.Combine(sourceDir, "gradle", "wrapper", "gradle-wrapper.properties");
        if (File.Exists(srcProps))
            File.Copy(srcProps, Path.Combine(wrapperDir, "gradle-wrapper.properties"), overwrite: true);

        // Copy gradlew scripts
        var srcGradlew = Path.Combine(sourceDir, "gradlew");
        if (File.Exists(srcGradlew))
            File.Copy(srcGradlew, Path.Combine(templatePath, "gradlew"), overwrite: true);

        var srcBat = Path.Combine(sourceDir, "gradlew.bat");
        if (File.Exists(srcBat))
            File.Copy(srcBat, Path.Combine(templatePath, "gradlew.bat"), overwrite: true);
        else
            WriteGradlewBat(templatePath); // Generate if not bundled
    }

    private static void WriteGradleWrapperProperties(string templatePath, string gradleVersion)
    {
        var wrapperDir = Path.Combine(templatePath, "gradle", "wrapper");
        Directory.CreateDirectory(wrapperDir);

        var content = $"""
            distributionBase=GRADLE_USER_HOME
            distributionPath=wrapper/dists
            distributionUrl=https\://services.gradle.org/distributions/gradle-{gradleVersion}-bin.zip
            networkTimeout=10000
            validateDistributionUrl=true
            zipStoreBase=GRADLE_USER_HOME
            zipStorePath=wrapper/dists
            """;
        File.WriteAllText(Path.Combine(wrapperDir, "gradle-wrapper.properties"), content);
    }

    private static void WriteGradlew(string templatePath)
    {
        // Standard POSIX gradlew bootstrap script
        var content = """
            #!/bin/sh
            # Gradle wrapper bootstrap — auto-generated by CSCraft
            APP_HOME=$( cd -P "${0%"${0##*/}"}" > /dev/null && pwd )
            exec java -jar "$APP_HOME/gradle/wrapper/gradle-wrapper.jar" "$@"
            """;
        File.WriteAllText(Path.Combine(templatePath, "gradlew"), content);
    }

    private static void WriteGradlewBat(string templatePath)
    {
        var content = """
            @rem Gradle wrapper bootstrap - auto-generated by CSCraft
            @if "%DEBUG%"=="" @echo off
            set DIRNAME=%~dp0
            java -jar "%DIRNAME%gradle\wrapper\gradle-wrapper.jar" %*
            """;
        File.WriteAllText(Path.Combine(templatePath, "gradlew.bat"), content);
    }
}

/// <summary>
/// Parsed data from a [ModInfo] attribute.
/// </summary>
public record ModInfo(
    string Id,
    string Name,
    string Version,
    string Author,
    string Description,
    string MinecraftVersion,
    string PackageName,
    string ClassName          = "",
    // Manual version overrides (empty = use catalog)
    string YarnMappings       = "",
    string FabricLoaderVersion = "",
    string FabricApiVersion   = "",
    string LoomVersion        = ""
);
