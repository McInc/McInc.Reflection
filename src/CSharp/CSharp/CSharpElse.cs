using System.Text;

namespace McInc.CSharp
{
    internal class CSharpElse : CSharpScope,
        ICSharpScope
    {
        public CSharpElse(StringBuilder builder, ushort level) : base(builder, level,
            "else")
        {
        }

        protected override bool AppendNewLineAtClose { get; } = false;
    }
}