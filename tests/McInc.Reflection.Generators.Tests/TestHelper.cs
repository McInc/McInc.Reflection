using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace McInc.Reflection.Generators.Tests;

public static class TestHelper
{
    public static Task Verify(string source)
    {
        // Parse the provided string into a C# syntax tree
        SyntaxTree syntaxTree = CSharpSyntaxTree.ParseText(source);

        // Create references for assemblies we require
        // We could add multiple references if required

        var dir = Path.GetDirectoryName(typeof(object).Assembly.Location);
        IEnumerable<PortableExecutableReference> references = new[]
        {
            MetadataReference.CreateFromFile(typeof(Attribute).Assembly.Location),
            MetadataReference.CreateFromFile(Path.Combine(dir, "System.Runtime.dll")),
            MetadataReference.CreateFromFile(typeof(System.Runtime.GCSettings).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(GenerateReflectionAttribute).Assembly.Location)
        };

        // Create a Roslyn compilation for the syntax tree.
        CSharpCompilation compilation = CSharpCompilation.Create(
            assemblyName: "Tests",
            syntaxTrees: new[] { syntaxTree },

            references: references,
            options: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary,
                reportSuppressedDiagnostics: true)
            {

            });

        var d = compilation.GetDiagnostics();
        if (d.Length > 0)
        {

        }

        // Create an instance of our EnumGenerator incremental source generator
        var generator = new ReflectionGenerator();

        // The GeneratorDriver is used to run our generator against a compilation
        GeneratorDriver driver = CSharpGeneratorDriver.Create(generator);

        // Run the source generator!
        driver = driver.RunGenerators(compilation);

        var aaa = driver.GetRunResult();


        // Use verify to snapshot test the source generator output!
        return Verifier.Verify(driver).UseDirectory("Snapshots"); ;
    }
}