using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace McInc.Reflection.Generators.Tests;

public static class TestHelper
{
    public static Task Verify(string source)
    {
        SyntaxTree syntaxTree = CSharpSyntaxTree.ParseText(source, 
            new CSharpParseOptions(LanguageVersion.CSharp10));

        var references = AppDomain.CurrentDomain.GetAssemblies()
            .Where(assembly => !assembly.IsDynamic && !string.IsNullOrWhiteSpace(assembly.Location))
            .Select(assembly => MetadataReference.CreateFromFile(assembly.Location))
            .Concat([
                MetadataReference.CreateFromFile(typeof(McInc.Reflection.GenerateReflectionAttribute).Assembly.Location)
                ]);

        var compilation = CSharpCompilation.Create(
                    assemblyName: "Tests",
                    syntaxTrees: new[] { syntaxTree },

                    references: references,
                    options: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary,
                        reportSuppressedDiagnostics: true)
                    {

                    });
        var generator = new ReflectionGenerator();

        //// The GeneratorDriver is used to run our generator against a compilation
        GeneratorDriver driver = CSharpGeneratorDriver.Create(generator);

        //// Run the source generator!
        driver = driver.RunGenerators(compilation);

        var driverResult = driver.GetRunResult();


        var combinedDiagnostics = driverResult.Diagnostics.AddRange(compilation.GetDiagnostics());
        Assert.Empty(combinedDiagnostics);
        return Verifier.Verify(driverResult).UseDirectory("Snapshots");
    }
}