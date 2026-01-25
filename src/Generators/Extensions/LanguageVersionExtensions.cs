using Microsoft.CodeAnalysis.CSharp;

namespace McInc.Reflection.Extensions;

internal static class LanguageVersionExtensions
{
    extension(LanguageVersion version)
    {
        public bool HasCollectionInitializers
            => version >= (LanguageVersion)1200;

        public bool HasNullableAnnotation
            => version >= LanguageVersion.CSharp8;

        public bool HasNullableReferenceAnnotation
            => version >= LanguageVersion.CSharp9;
    }
}