using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;

namespace BuildTask;

/// <summary>
/// MSBuild task that scans C# source files for [ModInfo(...)] attribute
/// and generates a complete Fabric template directory automatically.
///
/// This runs before the transpiler so the FabricTemplate exists when
/// the transpiler needs to write Java files into it.
/// </summary>
public class SetupFabricTemplate : Microsoft.Build.Utilities.Task
{
    /// <summary>The .cs source files to scan for [ModInfo].</summary>
    [Required]
    public ITaskItem[] SourceFiles { get; set; } = [];

    /// <summary>
    /// Base directory of the mod project (typically $(MSBuildProjectDirectory)).
    /// The FabricTemplate directory will be created under this.
    /// </summary>
    [Required]
    public string ProjectDirectory { get; set; } = "";

    /// <summary>
    /// Directory containing the bundled Gradle wrapper files (from the SDK NuGet).
    /// Contains gradlew, gradlew.bat, gradle/wrapper/gradle-wrapper.jar, etc.
    /// </summary>
    public string GradleWrapperSourceDir { get; set; } = "";

    // ── Outputs (feed into CSCraftTranspile) ──────────────────────────────────

    /// <summary>The generated FabricTemplate path (set only if ModInfo was found).</summary>
    [Output]
    public string FabricTemplatePath { get; set; } = "";

    /// <summary>The Java package name derived from ModInfo (e.g. "com.yourname.mymod").</summary>
    [Output]
    public string PackageName { get; set; } = "";

    /// <summary>The mod ID from ModInfo.</summary>
    [Output]
    public string ModId { get; set; } = "";

    /// <summary>Whether a [ModInfo] attribute was found.</summary>
    [Output]
    public bool ModInfoFound { get; set; }

    /// <summary>
    /// Auto-discovered JDK home path. Set when the SDK finds a suitable JDK
    /// (17 or 21) on the system. Empty if none found.
    /// </summary>
    [Output]
    public string DiscoveredJavaHome { get; set; } = "";

    /// <summary>
    /// Optional: the user's manual CSCraftJavaHome override, passed in from .targets
    /// so discovery can prioritize it.
    /// </summary>
    public string ManualJavaHome { get; set; } = "";

    public override bool Execute()
    {
        var sourcePaths = SourceFiles
            .Select(item => item.GetMetadata("FullPath"))
            .ToList();

        var info = FabricTemplateGenerator.ParseModInfo(sourcePaths);

        if (info == null)
        {
            ModInfoFound = false;
            Log.LogMessage(MessageImportance.Low,
                "CSCraft: No [ModInfo] attribute found — skipping template generation. " +
                "Add [ModInfo(Id = \"mymod\", Name = \"My Mod\")] to your mod class to enable auto-setup.");
            return true;
        }

        ModInfoFound = true;
        ModId = info.Id;
        PackageName = info.PackageName;

        string templatePath = Path.Combine(ProjectDirectory, "FabricTemplate");
        FabricTemplatePath = templatePath;

        Log.LogMessage(MessageImportance.High,
            $"CSCraft: [ModInfo] found — Id=\"{info.Id}\", Name=\"{info.Name}\", MC={info.MinecraftVersion}");

        // Warn if the version isn't in the catalog and no manual overrides were given
        bool hasOverrides = !string.IsNullOrWhiteSpace(info.YarnMappings)
                         || !string.IsNullOrWhiteSpace(info.FabricLoaderVersion)
                         || !string.IsNullOrWhiteSpace(info.FabricApiVersion)
                         || !string.IsNullOrWhiteSpace(info.LoomVersion);
        if (!FabricTemplateGenerator.IsKnownVersion(info.MinecraftVersion) && !hasOverrides)
        {
            Log.LogWarning(
                $"CSCraft: Minecraft version '{info.MinecraftVersion}' is not in the built-in catalog. " +
                "Falling back to nearest known version — Gradle build may fail. " +
                "Set YarnMappings, FabricLoaderVersion, FabricApiVersion, and LoomVersion in [ModInfo] " +
                "to target this version precisely (see https://fabricmc.net/develop).");
        }

        string? wrapperDir = !string.IsNullOrWhiteSpace(GradleWrapperSourceDir) && Directory.Exists(GradleWrapperSourceDir)
            ? GradleWrapperSourceDir
            : null;

        try
        {
            FabricTemplateGenerator.Generate(info, templatePath, wrapperDir);
            Log.LogMessage(MessageImportance.High,
                $"CSCraft: FabricTemplate ready at {templatePath}");
        }
        catch (Exception ex)
        {
            Log.LogError($"CSCraft: Failed to generate FabricTemplate: {ex.Message}");
            return false;
        }

        // ── Java Discovery ────────────────────────────────────────────────────
        DiscoverJava();

        return true;
    }

    private void DiscoverJava()
    {
        string? manual = !string.IsNullOrWhiteSpace(ManualJavaHome) ? ManualJavaHome : null;
        var jdk = JavaDiscovery.FindBestJdk(manual);

        if (jdk != null)
        {
            DiscoveredJavaHome = jdk.Path;
            Log.LogMessage(MessageImportance.High,
                $"CSCraft: Found JDK {jdk.MajorVersion} at {jdk.Path}");
        }
        else
        {
            Log.LogMessage(MessageImportance.High,
                "CSCraft: No suitable JDK found automatically. " +
                "Set <CSCraftJavaHome> in your .csproj or install JDK 17/21.");
        }
    }
}
