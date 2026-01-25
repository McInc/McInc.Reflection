using McInc.Reflection.Utilities;
using Microsoft.CodeAnalysis;

namespace McInc.Reflection.Info;

internal record TypeToGenerate
{
    public readonly Accessibility Accessibility;
    public readonly string FullName;
    public readonly bool IncludeInternals;
    public readonly bool IsWritable;
    public readonly EquatableArray<MemberToGenerate> Members;
    public readonly string Name;
    public readonly string Namespace;
    

    public TypeToGenerate(string name, string fullName, List<MemberToGenerate> members, 
        string @namespace, bool isWritable, Accessibility accessibility, bool includeInternals)
    {
        this.Name = name;
        this.FullName = fullName;
        this.Namespace = @namespace;
        this.IsWritable = isWritable;
        this.Accessibility = accessibility;
        this.IncludeInternals = includeInternals;
        this.Members = new EquatableArray<MemberToGenerate>(members.ToArray());
    }
};