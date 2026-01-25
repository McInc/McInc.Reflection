#nullable enable
using McInc.Reflection.Info;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace McInc.Reflection;

[Generator]
public class ReflectionGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var typesToGenerate = context.SyntaxProvider
            .CreateSyntaxProvider(
                // 👇 Runs for _every_ syntax node, on _every_ key press!
                predicate: static (s, _) => IsSyntaxTargetForGeneration(s),
                // 👇 Runs for _every_ node selected by the predicate, on _every_ key press!
                transform: static (ctx, _) => GetSemanticTargetForGeneration(ctx))
            .Where(static m => m is not null); // Filter out errors that we don't care about

        var languageVersions = context.CompilationProvider
            .Select((ctx, _) => (ctx as CSharpCompilation)?.LanguageVersion ?? LanguageVersion.CSharp7_3);


        var merged = typesToGenerate.Combine(languageVersions);

        context.RegisterSourceOutput(merged,
            static (spc, source) => Execute(source.Left, spc, source.Right));
    }

    private static bool IsSyntaxTargetForGeneration(SyntaxNode node)
        => node is TypeDeclarationSyntax { AttributeLists.Count: > 0 };


    private static TypeToGenerate? GetSemanticTargetForGeneration(GeneratorSyntaxContext context)
    {
        var typeDeclarationSyntax = (TypeDeclarationSyntax)context.Node;



        // ReSharper disable once ForeachCanBePartlyConvertedToQueryUsingAnotherGetEnumerator
        foreach (var attributeListSyntax in typeDeclarationSyntax.AttributeLists)
        {
            foreach (var attributeSyntax in attributeListSyntax.Attributes)
            {
                // weird, we couldn't get the symbol, ignore it
                if (ModelExtensions.GetSymbolInfo(context.SemanticModel, attributeSyntax).Symbol
                    is not IMethodSymbol attributeSymbol)
                    continue;

                var attributeContainingTypeSymbol = attributeSymbol.ContainingType;
                var fullName = attributeContainingTypeSymbol.ToDisplayString();

                if (fullName != "McInc.Reflection.GenerateReflectionAttribute")
                    continue;

                return GetTypeToGenerate(context.SemanticModel, typeDeclarationSyntax);
            }
        }

        return null;
    }

    private static TypeToGenerate? GetTypeToGenerate(SemanticModel semanticModel, TypeDeclarationSyntax typeDeclarationSyntax)
    {
        // Get the semantic representation of the type syntax
        if (ModelExtensions.GetDeclaredSymbol(semanticModel, typeDeclarationSyntax)
            is not INamedTypeSymbol typeSymbol)
        {
            // something went wrong
            return null;
        }

        var includeInternals = false;

        foreach (var attribute in typeSymbol.GetAttributes())
        {
            if (!"McInc.Reflection.GenerateReflectionAttribute".Equals(
                    attribute.AttributeClass?.ToDisplayString()))
                continue;

            foreach (var namedArgument in attribute.NamedArguments)
            {
                // Is this the ExtensionClassName argument?
                if (namedArgument.Key != nameof(GenerateReflectionAttribute.IncludeInternals)
                    || namedArgument.Value.Value is not bool incl)
                    continue;

                includeInternals = incl;
            }
        }

        // Get the full type name of the enum e.g. Colour, 
        // or OuterClass<T>.Colour if it was nested in a generic type (for example)
        var typeName = typeSymbol.Name;

        var typeFullName = typeSymbol.ToString();

        // Get all the members in the enum
        var typeMembers = typeSymbol.GetMembers();
        var members = new List<MemberToGenerate>();

        // Get all the fields from the enum, and add their name to the list
        foreach (var member in typeMembers)
        {
            if (member.IsStatic || member.DeclaredAccessibility == Accessibility.Private)
                continue;
            switch (member)
            {
                case IFieldSymbol field:

                    members.Add(new MemberToGenerate(field.Name, field.DeclaredAccessibility,
                        true,
                        field is { IsConst: false, IsReadOnly: false },
                        field.Type.ToString(),
                        field.Type.IsValueType));
                    break;
                case IPropertySymbol property:
                    members.Add(new MemberToGenerate(property.Name, property.DeclaredAccessibility,
                        property.GetMethod is not null
                        && (
                            property.GetMethod.DeclaredAccessibility == Accessibility.Public
                            || (
                                property.GetMethod.DeclaredAccessibility == Accessibility.Internal
                                && includeInternals
                            )
                        ),
                        property.SetMethod is not null
                        && !property.SetMethod.IsInitOnly
                        && (
                            property.SetMethod.DeclaredAccessibility == Accessibility.Public
                            || (
                                property.SetMethod.DeclaredAccessibility == Accessibility.Internal
                                && includeInternals
                            )
                        ),
                        property.Type.ToString(),
                        property.Type.IsValueType));
                    break;
                default:
                    var kind = member.Kind;
                    break;
            }
        }

        return new TypeToGenerate(typeName, typeFullName, members, GetNamespace(typeDeclarationSyntax),
            typeDeclarationSyntax.Kind() == SyntaxKind.ClassDeclaration,
            typeSymbol.DeclaredAccessibility, includeInternals);
    }


    // determine the namespace the class/enum/struct is declared in, if any
    private static string GetNamespace(BaseTypeDeclarationSyntax syntax)
    {
        // If we don't have a namespace at all we'll return an empty string
        // This accounts for the "default namespace" case
        var nameSpace = string.Empty;

        // Get the containing syntax node for the type declaration
        // (could be a nested type, for example)
        var potentialNamespaceParent = syntax.Parent;

        // Keep moving "out" of nested classes etc until we get to a namespace
        // or until we run out of parents
        while (potentialNamespaceParent != null &&
               potentialNamespaceParent is not NamespaceDeclarationSyntax
               && potentialNamespaceParent is not FileScopedNamespaceDeclarationSyntax)
        {
            potentialNamespaceParent = potentialNamespaceParent.Parent;
        }



        if (potentialNamespaceParent is not BaseNamespaceDeclarationSyntax namespaceParent)
            return nameSpace;

        // We have a namespace. Use that as the type
        nameSpace = namespaceParent.Name.ToString();

        // Keep moving "out" of the namespace declarations until we 
        // run out of nested namespace declarations
        while (true)
        {
            if (namespaceParent.Parent is not NamespaceDeclarationSyntax parent)
                break;

            // Add the outer namespace as a prefix to the final namespace
            nameSpace = $"{namespaceParent.Name}.{nameSpace}";
            namespaceParent = parent;
        }

        // return the final namespace
        return nameSpace;
    }


    private static void Execute(TypeToGenerate? type, SourceProductionContext context, LanguageVersion langVersion = LanguageVersion.CSharp7_3)
    {
        if (type is null)
            return;

        var (generatedTypeName, source) = SourceGeneratorHelper.GenerateType(type, langVersion);

        context.AddSource($"{generatedTypeName}.g.cs", source);
    }
}