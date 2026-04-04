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
    // Maps Minecraft version → (yarn mappings, loader version, fabric API version, loom version, gradle version)

    private static readonly Dictionary<string, (string Yarn, string Loader, string FabricApi, string Loom, string Gradle)> VersionTable = new()
    {
        ["1.21.1"] = ("1.21.1+build.3",   "0.18.6", "0.116.9+1.21.1",  "1.6.12", "8.10"),
        ["1.21"]   = ("1.21+build.2",      "0.18.6", "0.100.0+1.21",    "1.6.12", "8.10"),
        ["1.20.6"] = ("1.20.6+build.3",    "0.15.11","0.97.8+1.20.6",   "1.6.12", "8.10"),
        ["1.20.4"] = ("1.20.4+build.3",    "0.15.11","0.97.2+1.20.4",   "1.5.8",  "8.6"),
        ["1.20.2"] = ("1.20.2+build.4",    "0.15.11","0.91.6+1.20.2",   "1.5.8",  "8.6"),
        ["1.20.1"] = ("1.20.1+build.10",   "0.15.11","0.92.2+1.20.1",   "1.5.8",  "8.6"),
    };

    // ── Public API ────────────────────────────────────────────────────────────

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
        Directory.CreateDirectory(templatePath);

        var versions = GetVersions(info.MinecraftVersion);
        bool isFirstRun = !File.Exists(Path.Combine(templatePath, "build.gradle"));

        // Always regenerate these (they're derived from ModInfo)
        WriteGradleProperties(templatePath, info, versions);
        WriteFabricModJson(templatePath, info);

        if (isFirstRun)
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
                // Generate wrapper properties at minimum; user will need to provide gradle-wrapper.jar
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

        return new ModInfo(id, name, version, author, desc, mcVer, pkg);
    }

    // ── Version resolution ────────────────────────────────────────────────────

    private static (string Yarn, string Loader, string FabricApi, string Loom, string Gradle) GetVersions(string mcVersion)
    {
        if (VersionTable.TryGetValue(mcVersion, out var v))
            return v;

        // Fallback to 1.21.1 defaults
        return VersionTable["1.21.1"];
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
        // Determine the entry point class name from the package + mod ID
        string entryClass = $"{info.PackageName}.{char.ToUpper(info.Id[0])}{info.Id[1..]}";

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
    string PackageName
);
