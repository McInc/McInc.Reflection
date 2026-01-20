using System;
using System.Collections.Generic;
using System.Text;

namespace McInc.CSharp
{
    internal class CSharpCase : CSharpScope,
        ICSharpScope
    {
        public CSharpCase(string label, StringBuilder builder, ushort level) : base(builder, level, $"case {label}:")
        {
        }

        protected override IReadOnlyList<string> CloseLines { get; } = Array.Empty<string>();
        protected override IReadOnlyList<string> OpenLines { get; } = Array.Empty<string>();
    }
}