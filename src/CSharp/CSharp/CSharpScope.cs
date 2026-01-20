using System.Collections.Generic;
using System.Text;

namespace McInc.CSharp
{
    internal class CSharpScope : CSharpImplementation,
        ICSharpScope
    {
        public CSharpScope(StringBuilder builder, ushort level, string firstLine = "")
            : base(builder, level, true, firstLine)
        {
        }

        protected override IReadOnlyList<string> CloseLines { get; } = new[] { "}" };
        protected override IReadOnlyList<string> OpenLines { get; } = new[] { "{" };
    }
}