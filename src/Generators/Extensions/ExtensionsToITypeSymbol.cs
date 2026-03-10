using System.Data;
using Microsoft.CodeAnalysis;

namespace McInc.Reflection.Extensions;

public static class ExtensionsToITypeSymbol
{
    extension(INamedTypeSymbol namedTypeSymbol)
    {
        public bool IsListOfTLike()
        {
            if (!namedTypeSymbol.IsGenericType)
                return false;

            var unbound = namedTypeSymbol.IsUnboundGenericType
                ? namedTypeSymbol
                : namedTypeSymbol.ConstructUnboundGenericType();

            switch (unbound.ConstructedFrom.SpecialType)
            {
                case SpecialType.System_Collections_Generic_IEnumerable_T:
                case SpecialType.System_Collections_Generic_IList_T:
                case SpecialType.System_Collections_Generic_ICollection_T:
                case SpecialType.System_Collections_Generic_IReadOnlyList_T:
                case SpecialType.System_Collections_Generic_IReadOnlyCollection_T:
                    return true;
            }

            foreach (var interfaceType in namedTypeSymbol.AllInterfaces)
            {
                if (interfaceType.IsListOfTLike())
                    return true;
            }

            return false;
        }
    }

    extension(ITypeSymbol typeSymbol)
    {


        public void Analyze()
        {
            if (typeSymbol is not INamedTypeSymbol namedTypeSymbol)
            {
                //todo: not supported
                return;
            }


            var typeKind = namedTypeSymbol.TypeKind;

            if (typeKind == TypeKind.Struct)
            {
                var analyzedType = typeSymbol;
                var nullable = false;
                if (namedTypeSymbol.IsGenericType)
                {
                    var unbound = namedTypeSymbol.ConstructUnboundGenericType();
                    if (unbound.ConstructedFrom.SpecialType == SpecialType.System_Nullable_T)
                    {
                        nullable = true;
                        analyzedType = namedTypeSymbol.TypeArguments[0];
                    }
                }

                switch (analyzedType.SpecialType)
                {
                    case SpecialType.System_Enum:
                        break;
                    case SpecialType.System_ValueType:
                        break;
                    case SpecialType.System_Boolean:
                        break;
                    case SpecialType.System_Char:
                        break;
                    case SpecialType.System_SByte:
                        break;
                    case SpecialType.System_Byte:
                        break;
                    case SpecialType.System_Int16:
                        break;
                    case SpecialType.System_UInt16:
                        break;
                    case SpecialType.System_Int32:
                        break;
                    case SpecialType.System_UInt32:
                        break;
                    case SpecialType.System_Int64:
                        break;
                    case SpecialType.System_UInt64:
                        break;
                    case SpecialType.System_Decimal:
                        break;
                    case SpecialType.System_Single:
                        break;
                    case SpecialType.System_Double:
                        break;
                    case SpecialType.System_IntPtr:
                        break;
                    case SpecialType.System_UIntPtr:
                        break;
                    case SpecialType.System_DateTime:
                        break;
                    case SpecialType.System_TypedReference:
                        break;
                    default:
                        //todo: unsupported
                        return;
                }
            }
            else if (typeKind == TypeKind.Array)
            {
                namedTypeSymbol.TypeArguments[0].Analyze();
                //todo: return ArrayTypeDef
                return;
            }
            else if (typeKind == TypeKind.Enum)
            {
                //todo: return EnumTypeDef
                return;
            }
            else if (typeKind == TypeKind.Class)
            {
                if (namedTypeSymbol.SpecialType == SpecialType.System_String)
                {
                    //todo: return StringTypeDef;
                    return;
                }

                if (!namedTypeSymbol.IsListOfTLike())
                {
                    //todo: ClassTypeDef;
                    return;
                }

                if (!namedTypeSymbol.IsGenericType || namedTypeSymbol.IsUnboundGenericType)
                {
                    //todo: unsupported
                    return;
                }

                namedTypeSymbol.TypeArguments[0].Analyze();
                //todo: return ListTypeDef
                return;
            }

            //todo: unsupported
            return;


            //definire qui funzioni locali per compattare il codice
        }
    }
}