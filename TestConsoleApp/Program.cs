using System.Diagnostics;
using Transpiler;

Console.WriteLine("=== CSCraft Transpiler ===\n");

// ── Inputs ────────────────────────────────────────────────────────────────────

Console.Write("Mod .cs file path: ");
string modPath = Console.ReadLine()!.Trim().Trim('"');

if (!File.Exists(modPath))
{
    Console.Error.WriteLine($"File not found: {modPath}");
    return 1;
}

Console.Write("Package name (e.g. com.yourname.mymod): ");
string packageInput = Console.ReadLine()!.Trim();
string packageName  = string.IsNullOrWhiteSpace(packageInput) ? "com.example.mymod" : packageInput;

Console.Write("FabricTemplate path (leave blank to skip Gradle build): ");
string fabricInput = Console.ReadLine()!.Trim().Trim('"');
string? fabricPath = string.IsNullOrWhiteSpace(fabricInput) ? null : fabricInput;

// ── Transpile ─────────────────────────────────────────────────────────────────

Console.WriteLine("\nTranspiling...");
string source = File.ReadAllText(modPath);
var result    = TranspilerRunner.Transpile(source, packageName);

foreach (var w in result.Warnings)
    Console.WriteLine($"  WARN:  {w}");

if (result.Errors.Count > 0)
{
    foreach (var e in result.Errors)
        Console.Error.WriteLine($"  ERROR: {e}");
    Console.Error.WriteLine("\nTranspilation failed — no files written.");
    return 1;
}

// ── Write Java file ───────────────────────────────────────────────────────────

string className = Path.GetFileNameWithoutExtension(modPath);

string javaPath;
if (fabricPath != null)
{
    // Place the file in the correct package folder under src/main/java
    string packageFolder = packageName.Replace('.', Path.DirectorySeparatorChar);
    string javaDir       = Path.Combine(fabricPath, "src", "main", "java", packageFolder);
    Directory.CreateDirectory(javaDir);
    javaPath = Path.Combine(javaDir, className + ".java");
}
else
{
    // Write next to the .cs file
    javaPath = Path.Combine(Path.GetDirectoryName(modPath)!, className + ".java");
}

File.WriteAllText(javaPath, result.JavaSource);
Console.WriteLine($"Java written → {javaPath}");

// ── Gradle build ──────────────────────────────────────────────────────────────

if (fabricPath == null)
{
    Console.WriteLine("\nSkipping Gradle build (no FabricTemplate path given).");
    Console.WriteLine("Done.");
    return 0;
}


Console.WriteLine("\nRunning Gradle build...");

string gradlew = Path.Combine(fabricPath, OperatingSystem.IsWindows() ? "gradlew.bat" : "gradlew");
if (!File.Exists(gradlew))
{
    Console.Error.WriteLine($"gradlew not found at {gradlew}");
    return 1;
}

var psi = new ProcessStartInfo
{
    FileName         = gradlew,
    Arguments        = "build",
    WorkingDirectory = fabricPath,
    RedirectStandardOutput = true,
    RedirectStandardError  = true,
    UseShellExecute  = false,
};

// Make gradlew executable on Linux/Mac
if (!OperatingSystem.IsWindows())
    Process.Start("chmod", $"+x {gradlew}")?.WaitForExit();

var gradle = Process.Start(psi)!;

gradle.OutputDataReceived += (_, e) => { if (e.Data != null) Console.WriteLine($"  {e.Data}"); };
gradle.ErrorDataReceived  += (_, e) => { if (e.Data != null) Console.WriteLine($"  {e.Data}"); };
gradle.BeginOutputReadLine();
gradle.BeginErrorReadLine();
gradle.WaitForExit();

if (gradle.ExitCode != 0)
{
    Console.Error.WriteLine("\nGradle build failed.");
    return 1;
}

// Find the built jar
var jars = Directory.GetFiles(Path.Combine(fabricPath, "build", "libs"), "*.jar")
    .Where(j => !j.EndsWith("-sources.jar"))
    .ToArray();

Console.WriteLine("\n=== Done! ===");
foreach (var jar in jars)
    Console.WriteLine($"Jar → {jar}");

return 0;
