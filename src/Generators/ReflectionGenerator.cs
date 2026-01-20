using System.Collections.Generic;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Immutable;
using System.Text;

namespace McInc.Reflection;

[Generator]
public class ReflectionGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        IncrementalValuesProvider<TypeToGenerate?> typesToGenerate = context.SyntaxProvider
            .CreateSyntaxProvider(
                // 👇 Runs for _every_ syntax node, on _every_ key press!
                predicate: static (s, _) => IsSyntaxTargetForGeneration(s),
                // 👇 Runs for _every_ node selected by the predicate, on _every_ key press!
                transform: static (ctx, _) => GetSemanticTargetForGeneration(ctx))
            .Where(static m => m is not null); // Filter out errors that we don't care about

        context.RegisterSourceOutput(typesToGenerate,
            static (spc, source) => Execute(source, spc));
    }

    private static bool IsSyntaxTargetForGeneration(SyntaxNode node)
        =>
            node is TypeDeclarationSyntax { AttributeLists.Count: > 0 };


    static TypeToGenerate? GetSemanticTargetForGeneration(GeneratorSyntaxContext context)
    {
        // we know the node is a EnumDeclarationSyntax thanks to IsSyntaxTargetForGeneration
        var typeDeclarationSyntax = (TypeDeclarationSyntax)context.Node;

        // loop through all the attributes on the method
        foreach (AttributeListSyntax attributeListSyntax in typeDeclarationSyntax.AttributeLists)
        {
            foreach (AttributeSyntax attributeSyntax in attributeListSyntax.Attributes)
            {
                var si = context.SemanticModel.GetSymbolInfo(attributeSyntax);
                if (/*context.SemanticModel.GetSymbolInfo(attributeSyntax).Symbol*/ si.Symbol is not IMethodSymbol attributeSymbol)
                {
                    // weird, we couldn't get the symbol, ignore it
                    continue;
                }

                INamedTypeSymbol attributeContainingTypeSymbol = attributeSymbol.ContainingType;
                string fullName = attributeContainingTypeSymbol.ToDisplayString();

                // Is the attribute the [EnumExtensions] attribute?
                if (fullName == "McInc.Reflection.GenerateReflectionAttribute")
                {
                    // return the enum. Implementation shown in section 7.
                    return GetEnumToGenerate(context.SemanticModel, typeDeclarationSyntax);
                }
            }
        }

        // we didn't find the attribute we were looking for
        return null;
    }

    static TypeToGenerate? GetEnumToGenerate(SemanticModel semanticModel, TypeDeclarationSyntax typeDeclarationSyntax)
    {
        // Get the semantic representation of the enum syntax
        if (semanticModel.GetDeclaredSymbol(typeDeclarationSyntax) is not INamedTypeSymbol typeSymbol)
        {
            // something went wrong
            return null;
        }

        // Get the full type name of the enum e.g. Colour, 
        // or OuterClass<T>.Colour if it was nested in a generic type (for example)
        var typeName = typeSymbol.Name;
        string typeFullName = typeSymbol.ToString();

        // Get all the members in the enum
        ImmutableArray<ISymbol> typeMembers = typeSymbol.GetMembers();
        var members = new List<MemberToGenerate>();

        // Get all the fields from the enum, and add their name to the list
        foreach (ISymbol member in typeMembers)
        {
            switch (member)
            {
                case IFieldSymbol field:
                    members.Add(new MemberToGenerate(field.Name, field.DeclaredAccessibility,
                        true,
                        field is { IsConst: false, IsReadOnly: false },
                        field.IsStatic));
                    break;
                case IPropertySymbol property:
                    members.Add(new MemberToGenerate(property.Name, property.DeclaredAccessibility,
                        property.GetMethod?.DeclaredAccessibility == Accessibility.Public,
                        property.SetMethod?.DeclaredAccessibility == Accessibility.Public,
                        property.IsStatic));
                    break;
                default:
                    var kind = member.Kind;
                    break;
            }
        }

        return new TypeToGenerate(typeName, typeFullName, members, GetNamespace(typeDeclarationSyntax));
        ;
    }


    // determine the namespace the class/enum/struct is declared in, if any
    static string GetNamespace(BaseTypeDeclarationSyntax syntax)
    {
        // If we don't have a namespace at all we'll return an empty string
        // This accounts for the "default namespace" case
        string nameSpace = string.Empty;

        // Get the containing syntax node for the type declaration
        // (could be a nested type, for example)
        SyntaxNode? potentialNamespaceParent = syntax.Parent;

        // Keep moving "out" of nested classes etc until we get to a namespace
        // or until we run out of parents
        while (potentialNamespaceParent != null &&
               potentialNamespaceParent is not NamespaceDeclarationSyntax
               && potentialNamespaceParent is not FileScopedNamespaceDeclarationSyntax)
        {
            potentialNamespaceParent = potentialNamespaceParent.Parent;
        }

        // Build up the final namespace by looping until we no longer have a namespace declaration
        if (potentialNamespaceParent is BaseNamespaceDeclarationSyntax namespaceParent)
        {
            // We have a namespace. Use that as the type
            nameSpace = namespaceParent.Name.ToString();

            // Keep moving "out" of the namespace declarations until we 
            // run out of nested namespace declarations
            while (true)
            {
                if (namespaceParent.Parent is not NamespaceDeclarationSyntax parent)
                {
                    break;
                }

                // Add the outer namespace as a prefix to the final namespace
                nameSpace = $"{namespaceParent.Name}.{nameSpace}";
                namespaceParent = parent;
            }
        }

        // return the final namespace
        return nameSpace;
    }


    private static void Execute(TypeToGenerate? type, SourceProductionContext context)
    {
        if (type is null)
            return;


        var typeReflectionClassName = $"{type.Name}TypeReflection";

        var builder = new StringBuilder();

        var publicInstanceMembers = new List<MemberToGenerate>();
        foreach (var member in type.Members)
        {
            if (member.IsStatic || member.DeclaredAccessibility != Accessibility.Public)
                continue;
            publicInstanceMembers.Add(member);
        }

        using (var @namespace = CSharpCode.BeginNamespace(type.Namespace, builder))
        {

            using (var @class =
                   @namespace.BeginClass(Accessibility.Internal, CSharpCode.Modifiers.Sealed, typeReflectionClassName))
            {
                using (var constructor = @class.BeginConstructor(Accessibility.Public, $"{type.FullName} @object"))
                    @constructor.AddLine("this._object = @object;");


                using (var method = @class.BeginMethod(Accessibility.Public, CSharpCode.Modifiers.None, "bool",
                           "TryGetValue", "string name", "out object? value"))
                {
                    using (var @switch = method.BeginSwitch("name"))
                    {
                        foreach (var member in publicInstanceMembers)
                        {
                            if (member.IsStatic || !member.CanGet ||
                                member.DeclaredAccessibility != Accessibility.Public)
                                continue;

                            using var @case = @switch.BeginCase($"nameof({type.FullName}.{member.Name})");
                            @case.AddLine($"value = this._object.{member.Name};");
                            @case.AddLine("return true;");
                        }

                        using (var @default = @switch.BeginDefault())
                        {
                            @default.AddLine("value = null;");
                            @default.AddLine("return false;");
                        }
                    }
                }


                using (var method = @class.BeginMethod(Accessibility.Public, CSharpCode.Modifiers.None, "bool",
                           "TryGetValue<TValue>", "string name", "out TValue? value"))
                {
                    method.AddLine("if(!this.TryGetValue(name, out var v) || v is not TValue tv)");
                    using (var scope = method.BeginScope())
                    {
                        scope.AddLine("value = default;");
                        scope.AddLine("return false");
                    }

                    method.AddLine("value = tv");
                    method.AddLine("return true;");
                }

                @class.AddLine($"private readonly {type.FullName} _object;");


            }

        }



        //builder
        //    .AppendLine($"namespace {type.Namespace};")
        //    .AppendLine();

        //#region Class

        //builder
        //    .AppendLine($"internal sealed class {type.Name}TypeReflection")
        //    .AppendLine("{");

        //#region Constructor

        //builder
        //    .Append("\t").AppendLine($"public {typeReflectionClassName}({type.FullName} @object)")
        //    .Append("\t").AppendLine("{")
        //    .Append("\t\t").AppendLine("this._object = @object;")
        //    .Append("\t").AppendLine("}")
        //    .AppendLine();

        //#endregion

        //var publicInstanceMembers = new List<MemberToGenerate>();
        //foreach (var member in type.Members)
        //{
        //    if (member.IsStatic || member.DeclaredAccessibility != Accessibility.Public)
        //        continue;
        //    publicInstanceMembers.Add(member);
        //}

        //#region Properties

        //builder
        //    .Append("\t").Append("public string[] Members { get; } = ");
        //if (publicInstanceMembers.Count > 0)
        //{
        //    builder.AppendLine("new string[] {");
        //    for (var i = 0; i < publicInstanceMembers.Count; i++)
        //    {
        //        builder
        //            .Append("\t\t").Append($"nameof({type.FullName}.{publicInstanceMembers[i].Name})");
        //        if ((i + 1) < publicInstanceMembers.Count)
        //            builder.Append(",");
        //        builder.AppendLine();
        //    }
        //    builder
        //        .Append("\t").AppendLine("};");
        //}
        //else
        //{
        //    builder.AppendLine("System.Array.Empty<string>();");
        //}

        //builder
        //    .AppendLine();

        //#endregion

        //#region Method TryGetValue(string name, out object? value)

        //builder
        //    .Append("\t").AppendLine("public bool TryGetValue(string name, out object? value)")
        //    .Append("\t").AppendLine("{")
        //    .Append("\t\t").AppendLine("switch(name)")
        //    .Append("\t\t").AppendLine("{");
        //foreach (var member in publicInstanceMembers)
        //{
        //    if (member.IsStatic || !member.CanGet || member.DeclaredAccessibility != Accessibility.Public)
        //        continue;

        //    builder
        //        .Append("\t\t\t").AppendLine($"case nameof({type.FullName}.{member.Name}):")
        //        .Append("\t\t\t\t").AppendLine($"value = this._object.{member.Name};")
        //        .Append("\t\t\t\t").AppendLine("return true;")
        //        .AppendLine();
        //}

        //builder
        //    .Append("\t\t\t").AppendLine($"default:")
        //    .Append("\t\t\t\t").AppendLine("value = null;")
        //    .Append("\t\t\t\t").AppendLine("return false;");

        //builder
        //    .Append("\t\t").AppendLine("}");

        //builder
        //    .Append("\t").AppendLine("}")
        //    .AppendLine();

        //#endregion

        //#region Method bool TryGetValue<T>(string name, out T value)

        //builder
        //    .Append("\t").AppendLine("public bool TryGetValue<TValue>(string name, out TValue? value)")
        //    .Append("\t").AppendLine("{")
        //    .Append("\t\t").AppendLine("if (!this.TryGetValue(name, out var objValue)")
        //    .Append("\t\t\t").AppendLine("|| objValue is not T v)")
        //    .Append("\t\t").AppendLine("{")
        //    .Append("\t\t\t").AppendLine("value = default;")
        //    .Append("\t\t\t").AppendLine("return false;")
        //    .Append("\t\t").AppendLine("}")
        //    .AppendLine()
        //    .Append("\t\t").AppendLine("value = v;")
        //    .Append("\t\t").AppendLine("return true;")
        //    .Append("\t").AppendLine("}")
        //    .AppendLine();

        //#endregion

        //#region Fields

        //builder
        //    .Append("\t\t").AppendLine($"private readonly {type.FullName} _object;")
        //    .AppendLine();

        //#endregion

        //builder
        //    .AppendLine("}");

        //#endregion
        context.AddSource($"{typeReflectionClassName}.g.cs", builder.ToString());
    }
}