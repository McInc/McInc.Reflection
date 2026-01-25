using Microsoft.CodeAnalysis.CSharp;
using System.CodeDom.Compiler;
using McInc.Reflection.Extensions;
#pragma warning disable IDE0290

namespace McInc.Reflection.Utilities;

internal class CodeWriter : IndentedTextWriter, IDisposable
{
    public CodeWriter(LanguageVersion languageVersion, TextWriter writer)
        : base(writer, "   ")
    {
        this._languageVersion = languageVersion;
    }

    public CodeWriter AppendNullable(string type)
    {
        this.Write(this._languageVersion.HasNullableAnnotation
            ? $"{type}?"
            : type);

        return this;
    }

    public CodeWriter AppendNullableRef(string type)
    {
        this.Write(this._languageVersion.HasNullableReferenceAnnotation
            ? $"{type}?"
            : type);
        return this;
    }

    private readonly LanguageVersion _languageVersion;
}