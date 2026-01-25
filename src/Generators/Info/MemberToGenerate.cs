using Microsoft.CodeAnalysis;

// ReSharper disable ConvertToPrimaryConstructor

namespace McInc.Reflection.Info;

internal record struct MemberToGenerate
{
    public readonly bool CanGet;
    public readonly bool CanSet;
    public readonly Accessibility DeclaredAccessibility;
    
    public readonly string Name;
    public readonly string Type;
    public readonly bool IsValueType;

    public MemberToGenerate(string name, Accessibility declaredAccessibility, 
        bool canGet, bool canSet, string type, bool isValueType)
    {
        this.Name = name;
        this.DeclaredAccessibility = declaredAccessibility;
        this.CanGet = canGet;
        this.CanSet = canSet;
        this.Type = type;
        this.IsValueType = isValueType;
    }
}