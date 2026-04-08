using CSCraft.Detranspiler;

if (args.Length < 2)
{
    Console.WriteLine("CSCraft Detranspiler — Convert a Java Fabric mod to a CSCraft C# project");
    Console.WriteLine();
    Console.WriteLine("Usage: cscraft-decompile <java-mod-folder> <output-folder>");
    Console.WriteLine();
    Console.WriteLine("Example:");
    Console.WriteLine("  cscraft-decompile ./MyJavaMod ./MyConvertedMod");
    Console.WriteLine();
    Console.WriteLine("The tool will:");
    Console.WriteLine("  - Convert .java mod files to .cs using the CSCraft API");
    Console.WriteLine("  - Parse fabric.mod.json for mod metadata");
    Console.WriteLine("  - Copy textures, lang files, sounds, recipes, loot tables");
    Console.WriteLine("  - Generate a ready-to-build .csproj referencing CSCraft.Sdk");
    return 1;
}

string inputDir  = Path.GetFullPath(args[0]);
string outputDir = Path.GetFullPath(args[1]);

if (!Directory.Exists(inputDir))
{
    Console.Error.WriteLine($"Error: Input folder not found: {inputDir}");
    return 1;
}

Console.WriteLine($"Input:  {inputDir}");
Console.WriteLine($"Output: {outputDir}");
Console.WriteLine();

try
{
    var engine = new DetranspilerEngine(inputDir, outputDir);
    engine.Run();
    Console.WriteLine();
    Console.WriteLine("Done! Your C# project is ready.");
    Console.WriteLine($"  cd \"{outputDir}\"");
    Console.WriteLine($"  dotnet build");
    return 0;
}
catch (Exception ex)
{
    Console.Error.WriteLine($"Fatal error: {ex.Message}");
    return 1;
}
