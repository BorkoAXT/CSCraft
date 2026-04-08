namespace CSCraft.Detranspiler;

/// <summary>
/// Generates the output CSCraft project structure:
/// - <ModName>.csproj
/// - NuGet.Config pointing to local feed (if needed)
/// </summary>
public class ProjectGenerator
{
    private readonly string _outputDir;
    private readonly string _modId;
    private readonly string _modName;
    private readonly string _modVersion;
    private readonly string _sdkVersion;

    public ProjectGenerator(string outputDir, string modId, string modName, string modVersion, string sdkVersion = "1.4.3")
    {
        _outputDir  = outputDir;
        _modId      = modId;
        _modName    = modName;
        _modVersion = modVersion;
        _sdkVersion = sdkVersion;
    }

    public void Generate()
    {
        Directory.CreateDirectory(_outputDir);

        string safeName = SanitizeName(_modName);
        WriteCsproj(safeName);
        Console.WriteLine($"  Generated {safeName}.csproj");
    }

    private void WriteCsproj(string name)
    {
        string path = Path.Combine(_outputDir, $"{name}.csproj");
        // Derive Java package name from modId
        string package = $"com.author.{_modId.Replace('-', '_').Replace(' ', '_').ToLower()}";

        File.WriteAllText(path, $"""
<Project Sdk="Microsoft.NET.Sdk">

    <PropertyGroup>
        <TargetFramework>net9.0</TargetFramework>
        <ImplicitUsings>enable</ImplicitUsings>
        <Nullable>enable</Nullable>

        <CSCraftPackageName>{package}</CSCraftPackageName>
        <CSCraftFabricPath>$(MSBuildProjectDirectory)\FabricTemplate</CSCraftFabricPath>
        <CSCraftJavaHome></CSCraftJavaHome>
        <CSCraftRunGrandle>true</CSCraftRunGrandle>
    </PropertyGroup>

    <ItemGroup>
        <PackageReference Include="CSCraft.Sdk" Version="{_sdkVersion}" />
    </ItemGroup>

</Project>
""");
    }

    private static string SanitizeName(string name)
    {
        var sb = new System.Text.StringBuilder();
        bool nextUpper = true;
        foreach (char c in name)
        {
            if (char.IsLetterOrDigit(c))
            {
                sb.Append(nextUpper ? char.ToUpper(c) : c);
                nextUpper = false;
            }
            else nextUpper = true;
        }
        return sb.Length > 0 ? sb.ToString() : "MyMod";
    }
}
