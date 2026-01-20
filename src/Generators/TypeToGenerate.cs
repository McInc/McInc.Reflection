using System;
using System.Collections.Generic;
using Microsoft.CodeAnalysis;

namespace McInc.Reflection;

internal record TypeToGenerate
{
    public readonly EquatableArray<MemberToGenerate> Members;
    public readonly string FullName;
    public readonly string Name;
    public readonly string Namespace;
    

    public TypeToGenerate(string name, string fullName, List<MemberToGenerate> members, string ns)
    {
        this.Name = name;
        this.FullName = fullName;
        this.Namespace = ns;
        this.Members = new EquatableArray<MemberToGenerate>(members.ToArray());
    }
};

internal record struct MemberToGenerate
{
    public readonly bool CanGet;
    public readonly bool CanSet;
    public readonly Accessibility DeclaredAccessibility;
    public readonly bool IsStatic;
    public readonly string Name;

    public MemberToGenerate(string name, Accessibility declaredAccessibility, 
        bool canGet, bool canSet, bool isStatic)
    {
        this.Name = name;
        this.DeclaredAccessibility = declaredAccessibility;
        this.CanGet = canGet;
        this.CanSet = canSet;
        this.IsStatic = isStatic;
    }
}