using System;
using System.Text;

namespace McInc.CSharp
{
    internal class CSharpIf : CSharpScope,
        ICSharpScope
    {
        public CSharpIf(CSharpIfElse type, string condition,
            StringBuilder builder, ushort level) : base(builder, level,
            GenerateFirstLine(type, condition))
        {
        }

        protected override bool AppendNewLineAtClose { get; } = false;

        private static string GenerateFirstLine(CSharpIfElse type, string condition)
        {
            switch (type)
            {
                case CSharpIfElse.OnlyIf:
                    return $"if({condition})";
                case CSharpIfElse.IfElse:
                    return $"else if({condition})";

                default:
                    throw new ArgumentOutOfRangeException(nameof(type), type, null);
            }
        }
    }
}